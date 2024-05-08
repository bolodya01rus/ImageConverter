using CommunityToolkit.WinUI.Notifications;

using ImageResizer.Contracts.Services;

using Windows.UI.Notifications;

namespace ImageResizer.Services;

public partial class ToastNotificationsService : IToastNotificationsService
{
    public ToastNotificationsService()
    {
    }

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}
