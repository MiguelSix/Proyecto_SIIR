$(document).ready(function () {
    if ($('.notifications-dropdown').length > 0) {
        function loadNotifications() {
            $.get('/Student/Notification/GetNotifications', function (notifications) {
                $('.notifications-container').empty();

                if (notifications.length === 0) {
                    $('.notifications-container').append(
                        '<div class="dropdown-item text-muted text-center">No hay notificaciones</div>'
                    );
                } else {
                    notifications.forEach(function (notification) {
                        var notificationHtml = `
                            <div class="dropdown-item ${!notification.isRead ? 'unread-notification' : ''}" 
                                 data-notification-id="${notification.id}">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div class="notification-message">${notification.message}</div>
                                    <button class="btn btn-sm text-danger delete-notification" 
                                            data-notification-id="${notification.id}">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                                <small class="notification-timestamp">
                                    ${new Date(notification.createdAt).toLocaleString()}
                                </small>
                            </div>`;
                        $('.notifications-container').append(notificationHtml);
                    });
                }
            });
        }

        function updateNotificationCount() {
            $.get('/Student/Notification/GetUnreadCount', function (data) {
                if (data.count > 0) {
                    $('.notification-count').text(data.count).removeClass('d-none');
                } else {
                    $('.notification-count').addClass('d-none');
                }
            });
        }

        // Carga inicial
        loadNotifications();
        updateNotificationCount();

        // Actualizar cada 30 segundos
        setInterval(function () {
            loadNotifications();
            updateNotificationCount();
        }, 30000);

        // Marcar como leída al hacer clic
        $(document).on('click', '.notifications-container .dropdown-item', function (e) {
            // Evitar que se ejecute si se hizo clic en el botón de eliminar
            if ($(e.target).closest('.delete-notification').length > 0) {
                return;
            }

            var notificationId = $(this).data('notification-id');
            var $notificationItem = $(this);

            $.post('/Student/Notification/MarkAsRead/' + notificationId, function (response) {
                if (response.success) {
                    $notificationItem.removeClass('unread-notification');
                    updateNotificationCount();
                }
            });
        });

        // Eliminar notificación
        $(document).on('click', '.delete-notification, .delete-notification *', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var notificationId = $(this).closest('.delete-notification').data('notification-id');
            var $notificationItem = $(this).closest('.dropdown-item');

            $.post('/Student/Notification/Delete/' + notificationId, function (response) {
                if (response.success) {
                    $notificationItem.fadeOut(300, function () {
                        $(this).remove();
                        updateNotificationCount();
                        // Verificar si quedan notificaciones
                        if ($('.notifications-container .dropdown-item').length === 0) {
                            $('.notifications-container').append(
                                '<div class="dropdown-item text-muted text-center">No hay notificaciones</div>'
                            );
                        }
                    });
                }
            });
        });

    }
});