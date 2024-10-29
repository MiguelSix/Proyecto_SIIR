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
                    return `<img src="../../../${imagen}" width="100"/>`
                },
                "width": "10%"
            },
            {
                "data": "name",
                "render": function (data, type, student) {
                    return student.name + " " + student.lastName + " " + student.secondLastName;
                },
                "width": "15%"
            },
            { "data": "career", "width": "25%" },
            { "data": "controlNumber", "width": "10%" },
            {
                "data": "id",
                "render": function (data, student) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Students/Edit/${data}" class="btn btn-success btn-sm text-white" style="width:80px;">
                                <i class="far fa-edit"></i> Editar
                            </a>
                            <a onclick="Delete('/Admin/Students/Delete/${data}')" class="btn btn-danger btn-sm text-white" style="width:80px;">
                                <i class="far fa-trash-alt"></i> Borrar
                            </a>
                            ${student.isCaptain ?
                            `<a onclick="changeCaptainStatus('/Admin/Students/UnassignCaptain/${data}')" class="btn btn-warning btn-sm text-white" style="width:160px;">
                                <i class="fas fa-user-minus"></i> Quitar Capitán
                            </a>` :
                            `<a onclick="changeCaptainStatus('/Admin/Students/AssignCaptain/${data}')" class="btn btn-primary btn-sm text-white" style="width:160px;">
                                <i class="fas fa-user-plus"></i> Asignar Capitán
                            </a>`}
                        </div>`;
                },
                "width": "35%"
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

function changeCaptainStatus(url) {
    $.ajax({
        url: url,
        type: "PUT",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload(); // Recarga la tabla para reflejar el cambio
            } else {
                toastr.error(data.message);
            }
        }
    });
}
