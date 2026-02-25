using TMPro;
using UnityEngine;

namespace ThiccTapeman.Utils
{
    public static class Text
    {
        // ---------------------------------------------------- //
        // Default variables                                    //
        // ---------------------------------------------------- //
        public static int sortingOrderDefault = 5000;
        public static TextAlignmentOptions textAlignmentDefault = TextAlignmentOptions.Center;
        public static TextAnchor textAnchorDefault = TextAnchor.MiddleCenter;
        public static Color textColorDefault = Color.white;
        public static int textFontSizeDefault = 5;

        // ---------------------------------------------------- //
        // World space text                                     //
        // ---------------------------------------------------- //
        #region World Text 

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="text">Text you want to be displayed</param>
        /// <param name="position">Position of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(string text, Vector3 position)
        {
            return CreateWorldText(null, text, position, textFontSizeDefault, textColorDefault, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="text">Text you want to be displayed</param>
        /// <param name="position">Position of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 position)
        {
            return CreateWorldText(parent, text, position, textFontSizeDefault, textColorDefault, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="text">Text you want to be displayed</param>
        /// <param name="position">Position of the text</param>
        /// <param name="fontSize">The FontSize of the Text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(string text, Vector3 position, int fontSize)
        {
            return CreateWorldText(null, text, position, fontSize, textColorDefault, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="text">Text you want to be displayed</param>
        /// <param name="position">Position of the text</param>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="fontSize">The FontSize of the Text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 position, int fontSize)
        {
            return CreateWorldText(parent, text, position, fontSize, textColorDefault, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="text">Text you want to be displayed</param>
        /// <param name="position">Position of the text</param>
        /// <param name="color">The color of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(string text, Vector3 position, Color color)
        {
            return CreateWorldText(null, text, position, textFontSizeDefault, color, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The local position relative to the parent</param>
        /// <param name="fontSize">The font size of the text</param>
        /// <param name="color">The color of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(string text, Vector3 localPosition, int fontSize, Color color)
        {
            return CreateWorldText(null, text, localPosition, textFontSizeDefault, color, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The local position relative to the parent</param>
        /// <param name="color">The color of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, Color color)
        {
            return CreateWorldText(parent, text, localPosition, textFontSizeDefault, color, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The local position relative to the parent</param>
        /// <param name="fontSize">The font size of the text</param>
        /// <param name="color">The color of the text</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color)
        {
            return CreateWorldText(null, text, localPosition, fontSize, color, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The local position relative to the parent</param>
        /// <param name="fontSize">The font size of the text</param>
        /// <param name="color">The color of the text</param>
        /// <param name="anchor">The text anchor style</param>
        /// <param name="alignment">The text alignment style</param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor anchor, TextAlignment alignment)
        {
            return CreateWorldText(parent, text, localPosition, fontSize, color, textAnchorDefault, textAlignmentDefault, sortingOrderDefault);
        }

        /// <summary>
        /// Creates a world text displayed at the position
        /// </summary>
        /// <param name="parent">The parent that the object will be assigned to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The local position relative to the parent</param>
        /// <param name="fontSize">The font size of the text</param>
        /// <param name="color">The color of the text</param>
        /// <param name="anchor">The text anchor style</param>
        /// <param name="alignment">The text alignment style</param>
        /// <param name="sortingOrder"></param>
        /// <returns>The TextMeshPro that were created so the text can be destroyed and or updated</returns>
        public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor anchor, TextAlignmentOptions alignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("world_text", typeof(TextMeshPro));
            Transform transform = gameObject.transform;

            transform.SetParent(parent, false);
            transform.localPosition = localPosition;

            TextMeshPro textMesh = transform.GetComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.alignment = alignment;
            textMesh.fontSize = fontSize;

            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        #endregion

        #region World Popup Text
        /// <summary>
        /// This will create a text popup at the selected position, and it will slowly fade out with the popuptime
        /// </summary>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="finalPopupPosition">The text's final position, it will start at (0, 0, 0) </param>
        /// <param name="popupTime">The time the text will show for</param>
        public static void CreateWorldTextPopup(string text, Vector3 finalPopupPosition, float popupTime)
        {
            CreateWorldTextPopup(null, text, Vector3.zero, 12, Color.white, finalPopupPosition, popupTime);
        }

        /// <summary>
        /// This will create a text popup at the selected position, and it will slowly fade out with the popuptime
        /// </summary>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The text's start position</param>
        /// <param name="finalPopupPosition">The text's final position</param>
        /// <param name="popupTime">The time the text will show for</param>
        public static void CreateWorldTextPopup(string text, Vector3 localPosition, Vector3 finalPopupPosition, float popupTime)
        {
            CreateWorldTextPopup(null, text, localPosition, 12, Color.white, finalPopupPosition, popupTime);
        }

        /// <summary>
        /// This will create a text popup at the selected position, and it will slowly fade out with the popuptime
        /// </summary>
        /// <param name="parent">The parent that the world text will be attached to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The text's start position</param>
        /// <param name="finalPopupPosition">The text's final position</param>
        /// <param name="popupTime">The time the text will show for</param>
        public static void CreateWorldTextPopup(Transform parent, string text, Vector3 localPosition, Vector3 finalPopupPosition, float popupTime)
        {
            CreateWorldTextPopup(parent, text, localPosition, 12, Color.white, finalPopupPosition, popupTime);
        }

        /// <summary>
        /// This will create a text popup at the selected position, and it will slowly fade out with the popuptime
        /// </summary>
        /// <param name="parent">The parent that the world text will be attached to</param>
        /// <param name="text">The text that will be displayed</param>
        /// <param name="localPosition">The text's start position</param>
        /// <param name="fontSize">The font size of the text</param>
        /// <param name="color">The color of the text</param>
        /// <param name="finalPopupPosition">The text's final position</param>
        /// <param name="popupTime">The time the text will show for</param>
        public static void CreateWorldTextPopup(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, Vector3 finalPopupPosition, float popupTime)
        {
            TextMeshPro textMesh = CreateWorldText(parent, text, localPosition, fontSize, color, TextAnchor.LowerLeft, TextAlignmentOptions.Center, sortingOrderDefault);
            Transform transform = textMesh.transform;

            Vector3 moveAmount = (finalPopupPosition - localPosition) / popupTime;
            
            FunctionUpdater.Create(delegate () {
                transform.position += moveAmount * Time.unscaledDeltaTime;
                popupTime -= Time.unscaledDeltaTime;

                if (popupTime <= 0f)
                {
                    Object.Destroy(transform.gameObject);
                    return true;
                }
                else
                {
                    return false;
                }
            }, "world_text_popup");
        }

        #endregion
        // ---------------------------------------------------- //
    }
}
