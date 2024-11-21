function approveDocument(id) {
    changeStatus(id, 'Approved');
}

function rejectDocument(id) {
    //if (confirm('¿Estás seguro de que deseas rechazar este documento?')) {
    //    changeStatus(id, 'Rejected');
    //}
    $('#rejectModal').modal('show');
    // Guardamos el ID del documento para usarlo cuando se envíe el formulario
    document.getElementById('documentId').value = id;
}

function submitReject() {
    const reason = document.getElementById('rejectionReason').value;
    if (!reason.trim()) {
        alert('Por favor, ingrese un motivo de rechazo.');
        return;
    }

    // Obtenemos el ID del documento y el motivo de rechazo
    const id = document.getElementById('documentId').value;
    changeStatus(id, 'Rejected', reason);

    // Cerramos el modal
    $('#rejectModal').modal('hide');

    // Limpiamos el campo de texto
    document.getElementById('rejectionReason').value = '';
}

function changeStatus(id, status, rejectionReason = null) {
    document.getElementById('documentId').value = id;
    document.getElementById('documentStatus').value = status;

    if (status === 'Rejected') {
        if (!rejectionReason) {
            alert('Se requiere un motivo de rechazo.');
            return;
        }
        document.getElementById('rejectionReasonInput').value = rejectionReason;
    } else {
        document.getElementById('rejectionReasonInput').value = '';
    }

    document.getElementById('statusForm').submit();
}

$(document).ready(function () {
    $('#rejectModal').on('hidden.bs.modal', function () {
        document.getElementById('rejectionReason').value = '';
    });
    // Aseguramos que el modal use la versión correcta de Bootstrap
    $('#rejectModal').modal({
        keyboard: true,
        backdrop: 'static'
    });
});