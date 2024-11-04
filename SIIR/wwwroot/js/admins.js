
var dataTable;
$(document).ready(function () {
    cargarDatatable();
});
function cargarDatatable() {
    dataTable = $("#tblAdmins").DataTable({
        responsive: {
            details: {
                display: $.fn.dataTable.Responsive.display.childRow
            }
        },
        "ajax": {
            "url": "/Admin/Admins/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "id",
                "width": "5%",
                "responsivePriority": 1
            },
            {
                "data": "name",
                "width": "30%",
                "responsivePriority": 2
            },
            {
                "data": "lastName",
                "width": "30%",
                "responsivePriority": 2
            },
            {
                "data": "secondLastName",
                "width": "30%",
                "responsivePriority": 2
            }
        ],
        "language": {
            "decimal": "",
            "emptyTable": "No hay registros",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Entradas",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "Sin resultados encontrados",
            "paginate": {
                "first": "Primero",
                "last": "Ultimo",
                "next": "Siguiente",
                "previous": "Anterior"
            }
        },
        "order": [[0, "desc"]],
        "pageLength": 10,
        "lengthMenu": [[5, 10, 25, 50, -1], [5, 10, 25, 50, "Todos"]],
        "width": "100%"
    });
}