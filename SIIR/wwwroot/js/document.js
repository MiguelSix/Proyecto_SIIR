function cargarDatatable() {
    dataTable = $("#tblDocument").DataTable({
        "ajax": {
            "url": "/Student/Document/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "documentCatalog.name", "width": "40%" },  // Nombre del documento
            { "data": "updateDate", "width": "30%" },                 // Estado del documento
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Student/Document/View/${data}" class="btn btn-info btn-sm mx-1">Ver</a>
                                <a href="/Student/Document/Download/${data}" class="btn btn-secondary btn-sm mx-1">Descargar</a>
                                <a href="/Student/Document/Upload/${data}" class="btn btn-primary btn-sm mx-1">Subir</a>
                            </div>`;
                },
                "width": "15%"  // Espacio reducido para las acciones
            }
        ],
        "language": {
            // Configuración del lenguaje en español
        },
        "width": "100%"
    });
}
