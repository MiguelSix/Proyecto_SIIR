$(document).ready(function () {
    // Configuración de toastr
    function toastrConfiguration() {
        toastr.options = {
            "closeButton": true,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    }

    // Configurar toastr al inicio
    toastrConfiguration();

    $("#btnEnviarRecordatorios").click(function () {
        swal({
            title: "¿Enviar recordatorios?",
            text: "Se enviará un correo a todos los estudiantes que tengan documentos pendientes",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Sí, enviar!",
            cancelButtonText: "Cancelar",
            closeOnConfirm: true,
            closeOnCancel: true
        }, function (isConfirm) {
            if (isConfirm) {
                // Obtener el token antiforgery
                var token = $('input[name="__RequestVerificationToken"]').val();

                $.ajax({
                    url: '/Admin/DocumentCatalog/SendReminders',
                    type: 'POST',
                    headers: {
                        "RequestVerificationToken": token
                    },
                    success: function (response) {
                        console.log('Respuesta del servidor:', response);
                        if (response.success) {
                            toastr.success(response.message || 'Recordatorios enviados exitosamente');
                        } else {
                            toastr.error(response.message || 'Ha ocurrido un error al enviar los recordatorios');
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error en la petición:', {
                            status: status,
                            error: error,
                            response: xhr.responseText
                        });
                        toastr.error('Ha ocurrido un error al enviar los recordatorios');
                    }
                });
            }
        });
    });
});