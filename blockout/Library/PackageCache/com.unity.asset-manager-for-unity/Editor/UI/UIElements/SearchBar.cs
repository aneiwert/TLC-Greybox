using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class SearchBar : VisualElement
    {
        const string k_SearchTerms = "Search";
        const string k_SearchBarAssetName = "SearchBar";
        const string k_SearchCancelUssName = "search-clear";
        const string k_SearchFieldElementName = "inputSearch";
        const string k_PlaceholderClass = k_SearchBarAssetName + "__placeholder";
        const int k_MaxLength = 1024;

        readonly IPageManager m_PageManager;
        readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;
        readonly IMessageManager m_MessageManager;

        readonly VisualElement m_TextInput;

        bool m_Focused;

        Button m_ClearAllButton;
        VisualElement m_SearchChipsContainer;
        ScrollView m_SearchChipsScrollView;
        TextField m_SearchTextField;
        ToolbarSearchField m_ToolbarSearchField;

        IPageFilterStrategy PageFilterStrategy => m_PageManager?.PageFilterStrategy;

        public SearchBar(IPageManager pageManager, IProjectOrganizationProvider projectOrganizationProvider,
            IMessageManager messageManager)
        {
            m_PageManager = pageManager;
            m_ProjectOrganizationProvider = projectOrganizationProvider;
            m_MessageManager = messageManager;

            var windowContent = UIElementsUtils.LoadUXML(k_SearchBarAssetName);
            windowContent.CloneTree(this);

            m_ToolbarSearchField = UIElementsUtils.SetupToolbarSearchField(k_SearchFieldElementName, OnSearchbarValueChanged, this);
            m_SearchTextField = m_ToolbarSearchField.Q<TextField>();
            m_SearchTextField.maxLength = k_MaxLength;
            m_SearchTextField.isDelayed = true;

            m_ClearAllButton = m_ToolbarSearchField.Q<Button>("unity-cancel");
            if (m_ClearAllButton != null)
            {
                m_ClearAllButton.name = k_SearchCancelUssName;
            }

            m_SearchChipsScrollView = new ScrollView(ScrollViewMode.Horizontal);
            m_SearchChipsScrollView.AddToClassList("search-chip-scroll-view");
            m_SearchChipsScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            m_SearchChipsContainer = new VisualElement();
            m_SearchChipsContainer.AddToClassList("search-chip-container");

            m_SearchChipsScrollView.Add(m_SearchChipsContainer);
            m_ToolbarSearchField.Insert(1, m_SearchChipsScrollView);


            m_TextInput = m_SearchTextField.Q("unity-text-input");

            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            m_SearchTextField.AddToClassList(k_PlaceholderClass);
            ShowSearchTermsTextIfNeeded();

            InitDisplay(pageManager.ActivePage);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_SearchTextField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            m_SearchTextField.RegisterCallback<FocusOutEvent>(OnFocusOut);
            m_SearchTextField.RegisterCallback<FocusInEvent>(OnFocusIn);

            if (m_ClearAllButton != null)
            {
                m_ClearAllButton.clicked += OnSearchCancelClick;
            }

            m_PageManager.ActivePageChanged += OnActivePageChanged;
            m_PageManager.SearchFiltersChanged += OnPageSearchFiltersChanged;
            m_PageManager.LoadingStatusChanged += OnPageManagerLoadingStatusChanged;
            m_ProjectOrganizationProvider.OrganizationChanged += OnOrganizationChanged;
            m_MessageManager.GridViewMessageSet += OnGridViewMessageSet;
            m_MessageManager.GridViewMessageCleared += OnGridViewMessageCleared;

            var activePage = m_PageManager.ActivePage;
            Refresh(activePage);
            InitDisplay(activePage);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_SearchTextField.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            m_SearchTextField.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            m_SearchTextField.UnregisterCallback<FocusInEvent>(OnFocusIn);

            if (m_ClearAllButton != null)
            {
                m_ClearAllButton.clicked -= OnSearchCancelClick;
            }

            m_PageManager.ActivePageChanged -= OnActivePageChanged;
            m_PageManager.SearchFiltersChanged -= OnPageSearchFiltersChanged;
            m_PageManager.LoadingStatusChanged -= OnPageManagerLoadingStatusChanged;
            m_ProjectOrganizationProvider.OrganizationChanged -= OnOrganizationChanged;
            m_MessageManager.GridViewMessageSet -= OnGridViewMessageSet;
            m_MessageManager.GridViewMessageCleared -= OnGridViewMessageCleared;
        }

        void OnPageSearchFiltersChanged(IPage page, IEnumerable<string> searchFilters)
        {
            if (m_PageManager.IsActivePage(page))
            {
                Refresh(page);
            }
        }

        void OnPageManagerLoadingStatusChanged(IPage page, bool isLoading)
        {
            if (!m_PageManager.IsActivePage(page))
                return;

            if (!isLoading)
            {
                InitDisplay(page);
            }
        }

        void OnActivePageChanged(IPage page)
        {
            Refresh(page);
            InitDisplay(page);
        }

        void OnFocusIn(FocusInEvent evt)
        {
            m_Focused = true;
            if (m_SearchTextField.text == k_SearchTerms)
            {
                m_SearchTextField.SetValueWithoutNotify("");
                m_SearchTextField.RemoveFromClassList(k_PlaceholderClass);
            }
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            m_Focused = false;
            if (m_SearchChipsContainer.childCount == 0 && string.IsNullOrWhiteSpace(m_SearchTextField.text))
            {
                m_SearchTextField.AddToClassList(k_PlaceholderClass);
                ShowSearchTermsTextIfNeeded();
            }
        }

        void OnGridViewMessageSet(Message _)
        {
            Refresh(m_PageManager.ActivePage);
        }

        void OnGridViewMessageCleared()
        {
            Refresh(m_PageManager.ActivePage);
        }

        void InitDisplay(IPage page)
        {
            var display = page != null
                && m_ProjectOrganizationProvider.SelectedOrganization != null
                && m_ProjectOrganizationProvider.SelectedOrganization.ProjectInfos.Any();
            UIElementsUtils.SetDisplay(m_ToolbarSearchField, display);
        }

        void Refresh(IPage page)
        {
            if (page == null)
                return;

            m_SearchChipsContainer.Clear();
            m_ToolbarSearchField.SetValueWithoutNotify(string.Empty);
            foreach (var filter in PageFilterStrategy.SearchFilters)
            {
                m_SearchChipsContainer.Add(new SearchFilterChip(filter, DismissSearchFilter));
            }

            var hasChips = PageFilterStrategy.SearchFilters.Any();
            ShowSearchTermsTextIfNeeded();
            m_ClearAllButton.visible = hasChips;

            if (hasChips)
            {
                m_ToolbarSearchField.AddToClassList("has-chips");
                m_SearchChipsScrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

                void OnGeometryChanged(GeometryChangedEvent evt)
                {
                    m_SearchChipsScrollView.scrollOffset = new Vector2(m_SearchChipsScrollView.contentContainer.layout.width, 0);
                    m_SearchChipsScrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                }

                m_SearchChipsScrollView.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_ToolbarSearchField.RemoveFromClassList("has-chips");
                m_SearchChipsScrollView.style.display = DisplayStyle.None;
            }
        }

        void OnSearchCancelClick()
        {
            PageFilterStrategy.ClearSearchFilters();
            ShowSearchTermsTextIfNeeded();
        }

        void OnSearchbarValueChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == k_SearchTerms)
                return;

            var searchFilters = evt.newValue.Split(" ").Where(s => !string.IsNullOrEmpty(s));
            if (searchFilters.Any())
            {
                AnalyticsSender.SendEvent(new SearchAttemptEvent(searchFilters.Count()));
                PageFilterStrategy.AddSearchFilter(searchFilters);
            }

            if (m_Focused)
            {
                SetKeyboardFocusOnSearchField();
            }
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    m_ToolbarSearchField.value = "";
                    PageFilterStrategy.ClearSearchFilters();
                    SetKeyboardFocusOnSearchField();
                    return;
                case KeyCode.Backspace when string.IsNullOrWhiteSpace(m_SearchTextField.text):
                    PopSearchChip();
                    SetKeyboardFocusOnSearchField();
                    break;
            }
        }

        void SetKeyboardFocusOnSearchField()
        {
            m_TextInput.Focus();
        }

        void PopSearchChip()
        {
            if (!PageFilterStrategy.SearchFilters.Any())
                return;

            PageFilterStrategy.RemoveSearchFilter(PageFilterStrategy.SearchFilters.Last());
            if (!PageFilterStrategy.SearchFilters.Any())
            {
                ShowSearchTermsTextIfNeeded();
            }
        }

        void DismissSearchFilter(string searchFilter)
        {
            if (!PageFilterStrategy.SearchFilters.Contains(searchFilter))
                return;

            PageFilterStrategy.RemoveSearchFilter(searchFilter);
            SetKeyboardFocusOnSearchField();
        }

        void ShowSearchTermsTextIfNeeded()
        {
            if (m_SearchChipsContainer.childCount == 0 && !m_Focused)
            {
                m_SearchTextField.SetValueWithoutNotify(k_SearchTerms);
            }
        }

        void OnOrganizationChanged(OrganizationInfo organizationInfo)
        {
            var activePage = m_PageManager.ActivePage;
            Refresh(activePage);
            InitDisplay(activePage);
        }
    }
}
