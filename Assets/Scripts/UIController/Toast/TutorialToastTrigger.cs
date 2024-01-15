using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialToastTrigger : ToastTrigger
{
    [SerializeField, Tooltip("Text to replace in toast")]
    string Placeholder = "{key}";

    [SerializeField, Tooltip("Action from which will be retrieved the correspondent button name")]
    InputActionReference ActionReference;

    public override void Trigger(object obj)
    {
        toast.text = BuildText();
        base.Trigger(obj);
    }

    string BuildText()
    {
        if (!ActionReference)
            return string.Empty;

        string actionName = ActionToText();
        string text = toast.text.Replace(Placeholder, actionName);
        return text;
    }

    string ActionToText()
    {
        InputAction action = ActionReference.action;
        int bindingIndex = action.GetBindingIndex();

        string name = action.GetBindingDisplayString(bindingIndex);
        name = CheckMouseButtons(name);
        return name;
    }

    string CheckMouseButtons(string actionName)
    {
        if (actionName.ToUpper() == "LMB")
            return "Left mouse";
        if (actionName.ToUpper() == "RMB")
            return "Right mouse";
        return actionName;
    }
}

