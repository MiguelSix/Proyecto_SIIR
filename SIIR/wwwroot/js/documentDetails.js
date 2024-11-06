function approveDocument(id) {
    changeStatus(id, 'Approved');
}

function rejectDocument(id) {
    if (confirm('¿Estás seguro de que deseas rechazar este documento?')) {
        changeStatus(id, 'Rejected');
    }
}

function changeStatus(id, status) {
    document.getElementById('documentId').value = id;
    document.getElementById('documentStatus').value = status;
    document.getElementById('statusForm').submit();
}