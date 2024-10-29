var dataTable;

$(document).ready(function () {
    cargarDatatable();
});

function cargarDatatable(teamId) {
    dataTable = $("#tblRoster").DataTable({
        "ajax": {
            "url": "/admin/students/getStudentsByTeamId",
            "type": "GET",
            "datatype": "json",
            "data": { teamId: teamId }
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
