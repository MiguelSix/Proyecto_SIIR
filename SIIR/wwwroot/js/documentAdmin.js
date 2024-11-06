// Inicializar la tabla con DataTables
$(document).ready(function () {
    $('#documentsTable').DataTable({
        language: {
            "sProcessing": "Procesando...",
            "sLengthMenu": "Mostrar _MENU_ registros",
            "sZeroRecords": "No se encontraron resultados",
            "sEmptyTable": "Ningún dato disponible en esta tabla",
            "sInfo": "Mostrando registros del _START_ al _END_ de un total de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando registros del 0 al 0 de un total de 0 registros",
            "sInfoFiltered": "(filtrado de un total de _MAX_ registros)",
            "sInfoPostFix": "",
            "sSearch": "Buscar:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Cargando...",
            "oPaginate": {
                "sFirst": "Primero",
                "sLast": "Último",
                "sNext": "Siguiente",
                "sPrevious": "Anterior"
            },
            "oAria": {
                "sSortAscending": ": Activar para ordenar la columna de manera ascendente",
                "sSortDescending": ": Activar para ordenar la columna de manera descendente"
            }
        },
        order: [[1, 'desc']],
        pageLength: 10,
        columnDefs: [
            { orderable: false, targets: 3 } // Desactivar ordenamiento para la columna de acciones
        ]
    });
});

// Cambiar el estado de un documento mediante el formulario oculto
function changeStatus(id, status) {
    document.getElementById('documentId').value = id;
    document.getElementById('documentStatus').value = status;
    document.getElementById('statusForm').submit();
}