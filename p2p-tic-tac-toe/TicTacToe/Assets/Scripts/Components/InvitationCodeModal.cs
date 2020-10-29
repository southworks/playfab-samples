using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InvitationCodeModal : MonoBehaviour
{
    public GameObject InvitationCodeModalPnl;
    public Text InvitationCodeTxt;
    public InputField InvitationCodeInput;
    public Button CancelBtn;
    public Button OkBtn;
    public InvitationCodeModalResult Result = new InvitationCodeModalResult
    {
        OptionSelected = OptionSelected.NOT_OPTION,
        InvitationCode = ""
    };

    public enum OptionSelected
    {
        NOT_OPTION = -1,
        CANCEL = 0,
        OK = 1
    }

    public class InvitationCodeModalResult
    {
        public OptionSelected OptionSelected;
        public string InvitationCode;
    }

    public IEnumerator Show()
    {
        Initialize();
        yield return new WaitUntil(() => { return Result.OptionSelected != OptionSelected.NOT_OPTION; });
        InvitationCodeModalPnl.SetActive(false);
        yield return Result;
    }

    private void Initialize()
    {
        InvitationCodeModalPnl.SetActive(true);
        InvitationCodeInput.SetTextWithoutNotify("");
        CancelBtn.onClick.RemoveAllListeners();
        OkBtn.onClick.RemoveAllListeners();

        Result = new InvitationCodeModalResult
        {
            OptionSelected = OptionSelected.NOT_OPTION,
            InvitationCode = ""
        };

        OkBtn.onClick.AddListener(OkBtnOnClickListener);
        CancelBtn.onClick.AddListener(CancelBtnOnClickListener);
    }

    private void OkBtnOnClickListener()
    {
        Result.InvitationCode = InvitationCodeInput.text;
        Result.OptionSelected = OptionSelected.OK;
    }

    private void CancelBtnOnClickListener()
    {
        Result.InvitationCode = "";
        Result.OptionSelected = OptionSelected.CANCEL;
    }
}
