var dataTable;
$(document).ready(function () {
    loadCareers();
    cargarDatatable();
});

function loadCareers() {
    $.ajax({
        url: '/admin/students/GetCareers',
        type: 'GET',
        success: function (careers) {
            // Actualizar el select original
            var select = $('#careerFilter');
            careers.forEach(function (career) {
                select.append(new Option(career, career));
            });

            // Actualizar el contenedor de múltiple selección
            const $optionsList = $('.multi-select-container:eq(0) .multi-select-options');
            $optionsList.empty(); // Limpiar opciones existentes

            // Agregar las nuevas opciones como checkboxes
            careers.forEach(function (career) {
                const $option = $('<label/>', {
                    class: 'multi-select-option',
                    html: `<input type="checkbox" value="${career}"> 
                           <span>${career}</span>`
                });
                $optionsList.append($option);
            });

            // Reconfigurar los event listeners
            $optionsList.find('input[type="checkbox"]').on('change', function () {
                updateActiveFilters();
                if (window.dataTable) {
                    window.dataTable.ajax.reload();
                }
            });
        },
        error: function (error) {
            toastr.error("Error al cargar las carreras");
            console.error("Error loading careers:", error);
        }
    });
}

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
            "datatype": "json",
            "dataSrc": function (json) {
                const selectedFilters = window.selectedFilters || { careers: [], semesters: [] };
                return json.data.filter(function (item) {
                    const matchesCareer = selectedFilters.careers.length === 0 ||
                        selectedFilters.careers.includes(item.career);
                    const matchesSemester = selectedFilters.semesters.length === 0 ||
                        selectedFilters.semesters.includes(item.semester);
                    return matchesCareer && matchesSemester;
                });
            }
        },
        "columns": [
            {
                "data": "name",
                "responsivePriority": 1,
            },
            {
                "data": "lastName",
                "responsivePriority": 4
            },
            {
                "data": "secondLastName",
                "responsivePriority": 4
            },
            {
                "data": "imageUrl",
                "render": function (imageUrl) {
                    return `<img src="${imageUrl}" alt="Foto del estudiante" class="img-thumbnail" style="width: 100px;" onerror="this.onerror=null; this.src='/images/zorro_default.png';" />`;
                },
                "width": "5%",
                "responsivePriority": 4
            },
            {
                "data": "team",
                "render": function (data) {
                    return data ? `${data.name}` : 'N/A';
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
                "responsivePriority": 6
            },
            {
                "data": "career",
                "render": function (data) {
                    return data || 'Sin actualizar';
                },
                "responsivePriority": 6
            },
            {
                "data": "semester",
                "render": function (data) {
                    return data || 'Sin actualizar';
                },
                "responsivePriority": 6
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="/Admin/Students/Details/${data}" class="btn btn-success btn-sm text-white" style="cursor:pointer">
                                <i class="fas fa-eye"></i><span class="d-none d-sm-inline"> Ver</span>
                            </a>
                            <a onclick=Delete("/Admin/Students/Delete/${data}") class="btn btn-danger btn-sm text-white" style="cursor:pointer">
                                <i class="fas fa-trash-alt"></i><span class="d-none d-sm-inline"> Borrar</span>
                            </a>
                            <a onclick=downloadInfo("/Admin/Students/GenerateStudentCertificate/${data}") class="btn btn-info btn-sm text-white" style="cursor:pointer">
                                <i class="fas fa-download"></i><span class="d-none d-sm-inline"> Información</span>
                            </a>
                            <a href="/Admin/Document/Index?studentId=${data}" class="btn btn-secondary btn-sm text-white" style="cursor:pointer">
                                <i class="fas fa-file-alt"></i><span class="d-none d-sm-inline"> Documentos</span>
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

function downloadInfo(url) {
    toastr.options = {
        newestOnTop: false
    };
    toastr.info("Preparando descarga...");
    $.ajax({
        url: url,
        type: 'POST',
        xhrFields: { responseType: 'blob' },
        success: function (blob, status, xhr) {
            const disposition = xhr.getResponseHeader('Content-Disposition');
            let fileName = "Informacion_Estudiantes.pdf";

            if (disposition && disposition.includes('filename=')) {
                const filenameRegex = /filename[^;=\n]*=(['"]?)([^'"\n]*)\1/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[2]) {
                    fileName = matches[2];
                }
            }

            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName;
            link.click();
            window.URL.revokeObjectURL(link.href);

            toastr.success("Documento descargado correctamente.");
        },
        error: function (error) {
            toastr.error("Error al generar el documento de información del estudiante:", error);
        }
    });
}