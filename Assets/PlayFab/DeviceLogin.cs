using PlayFab.ClientModels;
using PlayFab;
public class DeviceLogin : ILogin
{
    private string deviceId;
    public DeviceLogin(string deviceId)
    {
        this.deviceId = deviceId;
    }
    public void Login(System.Action<LoginResult> onSuccess, System.Action<PlayFabError> onFailure)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = deviceId,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, onSuccess, onFailure);
    }
}
