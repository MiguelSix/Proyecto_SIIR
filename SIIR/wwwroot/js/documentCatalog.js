// Definición de la variable que contendrá la configuración de DataTable
var dataTable;

// Este fragmento de código jQuery asegura que la función 'cargarDatatable' 
// se ejecute una vez que el documento HTML haya sido completamente cargado
// en el navegador.
$(document).ready(function () {
    cargarDatatable();
});

// Función que configura y carga los datos en la tabla utilizando el plugin DataTables
function cargarDatatable() {
    // Configuración de la tabla utilizando el plugin DataTables
    dataTable = $("#tblDocumentCatalog").DataTable({
        // Configuración para realizar una solicitud AJAX y obtener los datos
        "ajax": {
            "url": "/Admin/DocumentCatalog/GetAll",  // URL donde se hará la solicitud para obtener los datos
            "type": "GET",                      // Tipo de solicitud HTTP (GET para obtener datos)
            "datatype": "json"                  // Tipo de datos esperados como respuesta
        },
        // Configuración de las columnas que se mostrarán en la tabla
        "columns": [
            { "data": "id", "width": "5%" },    // Columna para mostrar el ID, ocupando un 5% del ancho
            { "data": "nombre", "width": "20%" },  // Columna para el nombre, 40% del ancho
            { "data": "extension", "width": "10%" },   // Columna para el orden, 10% del ancho
            { "data": "descripcion", "width": "40%" },   // Columna para el orden, 10% del ancho
            {
                "data": "id",                    // Columna que también usará el 'id' para renderizar botones
                // Usamos 'render' para personalizar el contenido de esta columna
                "render": function (data) {
                    // Devuelve botones HTML para editar y borrar el registro, usando el 'id' del registro
                    return `<div class="text-center">
                                <a href="/Admin/DocumentCatalog/Edit/${data}" class="btn btn-success text-white" style="cursor:pointer; width:140px;">
                                <i class="far fa-edit"></i> Editar
                                </a>
                                &nbsp;
                                <a onclick=Delete("/Admin/DocumentCatalog/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer; width:140px;">
                                <i class="far fa-trash-alt"></i> Borrar
                                </a>
                          </div>`;
                }, "width": "40%"                // La columna de acciones ocupará el 40% del ancho
            }
        ],
        // Configuración de lenguaje y mensajes en español para la tabla
        "language": {
            "decimal": "",
            "emptyTable": "No hay registros",           // Mensaje cuando no hay datos en la tabla
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Entradas",  // Información de los registros mostrados
            "infoEmpty": "Mostrando 0 to 0 of 0 Entradas",  // Información cuando no hay entradas
            "infoFiltered": "(Filtrado de _MAX_ total entradas)",  // Mensaje de entradas filtradas
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Entradas",   // Texto del menú para seleccionar cuántas filas ver
            "loadingRecords": "Cargando...",          // Mensaje mientras se cargan los datos
            "processing": "Procesando...",            // Mensaje mientras se procesan datos
            "search": "Buscar:",                      // Texto para el campo de búsqueda
            "zeroRecords": "Sin resultados encontrados",  // Mensaje cuando no hay resultados en la búsqueda
            "paginate": {
                "first": "Primero",                   // Texto para el botón de 'Primero'
                "last": "Ultimo",                     // Texto para el botón de 'Último'
                "next": "Siguiente",                  // Texto para el botón de 'Siguiente'
                "previous": "Anterior"                // Texto para el botón de 'Anterior'
            }
        },
        "width": "100%"   // Configuración para que la tabla ocupe el 100% del ancho disponible
    });
}

// Función para borrar un registro
function Delete(url) {
    // Se usa SweetAlert para mostrar una confirmación antes de proceder con el borrado
    swal({
        title: "¿Está seguro de borrar?",              // Título del mensaje de advertencia
        text: "¡Este contenido no se puede recuperar!",  // Texto explicando que la acción es irreversible
        type: "warning",                               // Tipo de alerta (advertencia)
        showCancelButton: true,                        // Mostrar botón para cancelar la acción
        confirmButtonColor: "#DD6B55",                 // Color del botón de confirmación
        confirmButtonText: "Sí, borrar!",              // Texto del botón de confirmación
        closeOnconfirm: true                           // Cierra la alerta al confirmar
    }, function () {
        // Si se confirma, se hace una petición AJAX para borrar el registro
        $.ajax({
            type: 'DELETE',                            // Método HTTP DELETE para eliminar el registro
            url: url,                                  // URL a la que se enviará la solicitud de eliminación
            success: function (data) {                 // Función que se ejecuta al recibir una respuesta
                if (data.success) {
                    toastr.success(data.message);      // Muestra un mensaje de éxito con Toastr
                    dataTable.ajax.reload();           // Recarga la tabla para actualizar los datos
                } else {
                    toastr.error(data.message);        // Muestra un mensaje de error si ocurre un problema
                }
            }
        });
    });
}
