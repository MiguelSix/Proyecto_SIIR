var dataTable;
$(document).ready(function () {
    cargarDatatable();
});

function cargarDatatable() {
    dataTable = $("#tblStudents").DataTable({
        responsive: {
            details: {
                display: $.fn.dataTable.Responsive.display.childRow
            }
        },
        "ajax": {
            "url": "/admin/students/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "id",
                "responsivePriority": 3
            },
            {
                "data": "name",
                "responsivePriority": 1
            },
            {
                "data": "lastName",
                "responsivePriority": 3
            },
            {
                "data": "secondLastName",
                "responsivePriority": 3
            },
            {
                "data": "imageUrl",
                "render": function (imageUrl) {
                    return `<img src="${imageUrl}" alt="Foto del estudiante" class="img-fluid d-block mx-auto" style="max-width: 120px; max-height: 50px; object-fit: cover;" onerror="this.onerror=null; this.src='/images/zorro_default.png';" />`;
                },
                "responsivePriority": 4
            },
            {
                "data": "team",
                "render": function (data) {
                    return data ? `${data.name}  ${data.category}` : 'N/A';
                },
                "responsivePriority": 3
            },
            {
                "data": "coach",
                "render": function (data) {
                    if (data && (data.name || data.lastName)) {
                        return `${data.name || ''} ${data.lastName || ''}`.trim();
                    }
                    return 'N/A';
                },
                "responsivePriority": 4
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Students/Edit/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                <i class="fas fa-edit"></i> Editar
                            </a>
                            <a onclick=Delete("/Admin/Students/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                <i class="fas fa-trash-alt"></i> Borrar
                            </a>
                        </div>`;
                },
                "responsivePriority": 1
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
        confirmButtonText: "¡Sí, borrar!",
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