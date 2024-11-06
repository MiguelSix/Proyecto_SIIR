var dataTable;
$(document).ready(function () {
    cargarDatatable();
});
function cargarDatatable() {
    dataTable = $("#tblRepresentatives").DataTable({
        responsive: {
            details: {
                display: $.fn.dataTable.Responsive.display.childRow
            }
        },
        "ajax": {
            "url": "/admin/representatives/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "name",
                "width": "40%",
                "responsivePriority": 1
            },
            {
                "data": "category",
                "width": "10%",
                "responsivePriority": 4
            },
            {
                "data": "id",
                "responsivePriority": 1,
                "render": function (data) {
                    return `<div class="d-flex justify-content-center gap-2">
                                <a href="/Admin/Representatives/Edit/${data}" class="btn btn-success btn-sm text-white">
                                    <i class="far fa-edit"></i> Editar
                                </a>
                                <a onclick=Delete("/Admin/Representatives/Delete/${data}") class="btn btn-danger btn-sm text-white">
                                    <i class="far fa-trash-alt"></i> Borrar
                                </a>
                          </div>`;
                },
                "width": "40%"
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

function Delete(url) {
    swal({
        title: "¿Está seguro de borrar?",
        text: "¡Este contenido no se puede recuperar!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "¡Si, borrar!",
        closeOnconfirm: true
    }, function () {
        $.ajax({
            type: 'DELETE',
            url: url,
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    dataTable.ajax.reload();
                }
                else {
                    toastr.error(data.message);
                }
            }
        });
    });
}