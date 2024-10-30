var dataTable;

$(document).ready(function () {
    cargarDatatable();
});

function cargarDatatable() {
    dataTable = $("#tblTeams").DataTable({
        "ajax": {
            "url": "/admin/teams/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "3%" },
            { "data": "name", "width": "10%" },
            { "data": "category", "width": "8%" },
            {
                "data": "imageUrl",
                "render": function (imagen) {
                    return `<img src="../${imagen}" width="100"/>`
                },
                "width": "10%"
            },
            { "data": "coach.name", "width": "10%" },
            { "data": "representative.name", "width": "10%" },
            //{ "data": "student.name", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Teams/Edit/${data}" class="btn btn-success btn-sm text-white" style="width:80px;">
                                    <i class="far fa-edit"></i> Editar
                                </a>
                             
                                <a onclick=Delete("/Admin/Teams/Delete/${data}") class="btn btn-danger btn-sm text-white" style="width:80px;">
                                    <i class="far fa-trash-alt"></i> Borrar
                                </a>

                                <a href="/Admin/Teams/Roster/${data}" class="btn btn-info btn-sm text-white" style="width:140px;">
                                    <i class="fas fa-users"></i> Ver plantilla
                                </a>

                            </div>`;
                },
                "width": "29%"
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
        "width": "100%"
    });
}

function Delete(url) {
    swal({
        title: "Esta seguro de borrar?",
        text: "Este contenido no se puede recuperar!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Si, borrar!",
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