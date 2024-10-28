var dataTable;

$(document).ready(function () {
    cargarDatatable();
});

function cargarDatatable() {
    dataTable = $("#tblRoster").DataTable({
        "ajax": {
            "url": "/admin/students/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "imageUrl",
                "render": function (imagen) {
                    return `<img src="../${imagen}" width="100"/>`
                },
                "width": "10%"
            },
            //Ingresar nombre completo
            {
                "data": "name",
                "render": function (data, type, student) {
                    return student.name + " " + student.lastName + " " + student.secondLastName;
                },
                "width": "12%"
            },
            { "data": "career", "width": "20%" },
            { "data": "controlNumber", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Students/Edit/${data}" class="btn btn-success btn-sm text-white" style="width:80px;">
                                    <i class="far fa-edit"></i> Editar
                                </a>
                             
                                <a onclick=Delete("/Admin/Students/Delete/${data}") class="btn btn-danger btn-sm text-white" style="width:80px;">
                                    <i class="far fa-trash-alt"></i> Borrar
                                </a>

                            </div>`;
                },
                "width": "25%"
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