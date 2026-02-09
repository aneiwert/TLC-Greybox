//
// Ben Albers
// February 2026
// TextController.cs
//

using System.Xml;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

//TextController allows the user to cycle through a series of text messages on a text object.
public class TextController : MonoBehaviour
{
    //GameObject with TMP_Text Component
    public GameObject textObject;
    //TMP_Text component to manipulate
    public TMP_Text tmpText;
    //InputController component to reference
    public InputController inputController;
    //Strings for the TMP_Text component
    public string currentText;
    private string originalText;
    public string string1 = "Text One";
    public string string2 = "Text Two";
    public string string3 = "Text Three, once more to reset.";
    public string yourText;
    
    public TMP_FontAsset fontAsset;
    private TMP_FontAsset originalFontAsset;
    public FontStyles fontStyle;
    private FontStyles originalFontStyle;
    public float fontSize;
    private float originalFontSize;
    public Color textColor;
    private Color originalTextColor;
    public bool overrideTags;
    private bool originalOverrideTags;
    public float characterSpacing;
    private float originalCharacterSpacing;
    public float wordSpacing;
    private float originalWordSpacing;
    public float lineSpacing;
    private float originalLineSpacing;
    public float paragraphSpacing;
    private float originalParagraphSpacing;
    public float characterHorizontalScale;
    private float originalCharacterHorizontalScale;
    public TextAlignmentOptions alignment;
    private TextAlignmentOptions originalAlignment;
    public TextWrappingModes textWrappingMode;
    private TextWrappingModes originalTextWrappingMode;
    public TextOverflowModes textOverflowMode;
    private TextOverflowModes originalTextOverflowMode;
    public TextureMappingOptions horizontalMapping;
    private TextureMappingOptions originalHorizontalMapping;
    public TextureMappingOptions verticalMapping;
    private TextureMappingOptions originalVerticalMapping;

    //Start function assigns text object and TMP_Text component.
    void Start()
    {
        //if we don't have a textObject assigned,
        if (textObject == null)
        {
            //if this script is on an object with a TextMeshPro component,
            if (gameObject.GetComponent<TMP_Text>())
            {
                //assign this object as our textObject,
                textObject = gameObject;
                //and plug in all the corresponding variables (writeables and original readables) starting with text as tmpText.
                tmpText = gameObject.GetComponent<TMP_Text>();
                originalText = tmpText.text;
                if (fontAsset == null) { fontAsset = tmpText.font; }
                if (originalFontAsset == null) { originalFontAsset = tmpText.font; }
                if (fontStyle == FontStyles.Normal) { fontStyle = tmpText.fontStyle; }
                if (originalFontStyle == FontStyles.Normal) { originalFontStyle = tmpText.fontStyle; }
                if (fontSize == 0) { fontSize = tmpText.fontSize; }
                if (originalFontSize == 0) { originalFontSize = tmpText.fontSize; }
                textColor = tmpText.color;
                originalTextColor = tmpText.color;
                overrideTags = tmpText.overrideColorTags;
                originalOverrideTags = tmpText.overrideColorTags;
                characterSpacing = tmpText.characterSpacing;
                originalCharacterSpacing = tmpText.characterSpacing;
                wordSpacing = tmpText.wordSpacing;
                originalWordSpacing = tmpText.wordSpacing;
                lineSpacing = tmpText.lineSpacing;
                originalLineSpacing = tmpText.lineSpacing;
                paragraphSpacing = tmpText.paragraphSpacing;
                originalParagraphSpacing = tmpText.paragraphSpacing;
                if (characterHorizontalScale == 0) { characterHorizontalScale = tmpText.characterHorizontalScale; }
                if (originalCharacterHorizontalScale == 0) { originalCharacterHorizontalScale = tmpText.characterHorizontalScale; }
                alignment = tmpText.alignment;
                originalAlignment = tmpText.alignment;
                textWrappingMode = tmpText.textWrappingMode;
                originalTextWrappingMode = tmpText.textWrappingMode;
                textOverflowMode = tmpText.overflowMode;
                originalTextOverflowMode = tmpText.overflowMode;
                horizontalMapping = tmpText.horizontalMapping;
                originalHorizontalMapping = tmpText.horizontalMapping;
                verticalMapping = tmpText.verticalMapping;
                originalVerticalMapping = tmpText.verticalMapping;
            }
            else
            {
                //Debug.Log("no text object set");
            }
        }

        //assign or create input controller 
        if (inputController == null)
        {
            //if this script is on an object with an InputController,
            if (gameObject.GetComponent<InputController>())
            {
                //assign it as our inputController (InputController)
                inputController = gameObject.GetComponent<InputController>();
            }
            else
            {
                //or add a new one so we can access the Interact key
                inputController = gameObject.AddComponent<InputController>();
            }
        }
    }
    void Update()
    {
        //Comment out or delete TestStrings call if unused.
        if (inputController.interact)
        {
            TestStrings();
        }

        //if (gameObject.GetComponent<YourComponent>().yourVariable == yourCondition)
        {
            //YourTextFunction();
        }
    }

    //ResetText function restores all original variables of the gameObject's TextMeshPro component.
    void ResetText()
    {
        //if tmpText exists,
        if (tmpText != null)
        {
            //reset all variables of TextMeshPro component to their stored original values.
            tmpText.text = originalText;
            tmpText.font = originalFontAsset;
            tmpText.fontStyle = originalFontStyle;
            tmpText.fontSize = originalFontSize;
            tmpText.color = originalTextColor;
            tmpText.overrideColorTags = originalOverrideTags;
            tmpText.characterSpacing = originalCharacterSpacing;
            tmpText.wordSpacing = originalWordSpacing;
            tmpText.lineSpacing = originalLineSpacing;
            tmpText.paragraphSpacing = originalParagraphSpacing;
            tmpText.characterHorizontalScale = originalCharacterHorizontalScale;
            tmpText.alignment = originalAlignment;
            tmpText.textWrappingMode = originalTextWrappingMode;
            tmpText.overflowMode = originalTextOverflowMode;
            tmpText.horizontalMapping = originalHorizontalMapping;
            tmpText.verticalMapping = originalVerticalMapping;
        }
    }

    //The SetText function sets all variables of TextMeshPro component to their intended current values.
    void SetText()
    {
        //if tmpText component exists,
        if (tmpText != null)
        {
            //set all variables of TextMeshPro component to their intended current values.
            tmpText.text = currentText;
            tmpText.font = fontAsset;
            tmpText.fontStyle = fontStyle;
            tmpText.fontSize = fontSize;
            tmpText.color = textColor;
            tmpText.overrideColorTags = overrideTags;
            tmpText.characterSpacing = characterSpacing;
            tmpText.wordSpacing = wordSpacing;
            tmpText.lineSpacing = lineSpacing;
            tmpText.paragraphSpacing = paragraphSpacing;
            tmpText.characterHorizontalScale = characterHorizontalScale;
            tmpText.alignment = alignment;
            tmpText.textWrappingMode = textWrappingMode;
            tmpText.overflowMode = textOverflowMode;
            tmpText.horizontalMapping = horizontalMapping;
            tmpText.verticalMapping = verticalMapping;
        }
    }

    void YourTextFunction()
    {
        if (tmpText != null)
        {
            if (tmpText.text != yourText)
            { 
                currentText = yourText;
                SetText();
            }
        }
    }

    //TestStrings rotates through three strings of text, then resets the text field.
    void TestStrings()
    {
        //if we have a text component assigned,
        if (tmpText != null)
        {
            //checking for last possible string first
            if (tmpText.text == string3)
            {
                //and reseting to the saved original string when complete
                ResetText();
            }
            else if (tmpText.text == string2)
            {
                currentText = string3;
                SetText();
            }
            else if (tmpText.text == string1)
            {
                currentText = string2;
                textColor = Color.red;
                SetText();
            }
            //base case
            else
            {
                //setting text to our first string
                currentText = string1;
                SetText();
            }
        }
        else
        {
            //Debug.Log("No text object set");
            //Run Start() again to check for a new text object
            Start();
        }
    }
}
