var dataTable;

$(document).ready(function () {
    cargarDatatable();
});

function cargarDatatable() {
    dataTable = $("#tblDocumentCatalog").DataTable({
        responsive: {
            details: {
                display: $.fn.dataTable.Responsive.display.childRow
            }
        },
        "ajax": {
            "url": "/Admin/DocumentCatalog/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "id",
                "width": "5%",
                "responsivePriority": 3
            },
            {
                "data": "name",
                "width": "20%",
                "responsivePriority": 1
            },
            {
                "data": "extension",
                "width": "10%",
                "responsivePriority": 2
            },
            {
                "data": "description",
                "width": "40%",
                "responsivePriority": 4,
                "render": function (data) {
                    return `<div class="text-wrap" style="max-width: 300px; white-space: normal;">${data}</div>`;
                }
            },
            {
                "data": "id",
                "width": "40%",
                "responsivePriority": 1,
                "render": function (data) {
                    return `<div class="d-flex justify-content-center gap-2">
                                <a href="/Admin/DocumentCatalog/Edit/${data}" class="btn btn-success text-white">
                                    <i class="far fa-edit"></i> Editar
                                </a>
                                <a onclick=Delete("/Admin/DocumentCatalog/Delete/${data}") class="btn btn-danger text-white">
                                    <i class="far fa-trash-alt"></i> Borrar
                                </a>
                           </div>`;
                }
            }
        ],
        "language": {
            "decimal": "",
            "emptyTable": "No hay registros",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",
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
        confirmButtonText: "Sí, borrar!",
        closeOnconfirm: true
    }, function () {
        $.ajax({
            type: 'DELETE',
            url: url,
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    dataTable.ajax.reload();
                } else {
                    toastr.error(data.message);
                }
            }
        });
    });
}