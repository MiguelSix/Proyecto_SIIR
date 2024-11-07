let dataTable;
let dataTableTarjetas;
let currentCaptainId;
let allStudents = [];

$(document).ready(function () {
    const teamId = $("#teamId").val();

    // Store current captain ID if exists
    currentCaptainId = $("#captainId").val();

    // Handle change captain button click
    $("#changeCaptainBtn").click(function () {
        loadStudentsForCaptainSelection();
    });

    // Handle student selection for captain
    $("#confirmCaptainChange").click(function () {
        const selectedStudentId = $("#studentSelect").val();
        if (selectedStudentId) {
            changeCaptain(selectedStudentId);
        }
    });

    if (!teamId) {
        console.error('No se encontró el ID del equipo');
        return;
    }

    // Inicializar DataTable
    initializeDataTable(teamId);

    // Manejar el cambio de vista
    $('#viewSwitch').change(function () {
        if ($(this).is(':checked')) {
            // Cambiar a vista de tarjetas
            $('#tableView').hide();
            $('#cardView').show();
            generarTarjetas(teamId);
        } else {
            // Cambiar a vista de tabla
            $('#cardView').hide();
            $('#tableView').show();
            // Recargar la tabla si es necesario
            dataTable.ajax.reload();
        }
    });

    // Generar cedula modal
    $("#generateCertificateBtn").click(function () {
        loadStudentsForCertificate();
    });

    //Generar cedula
    $("#btn-generar-cedula").click(function () {
        generateCertificate();
    });
});

function loadStudentsForCaptainSelection() {
    const teamId = $("#teamId").val();
    $.ajax({
        url: `${getStudentsUrl}?teamId=${teamId}`,
        type: "GET",
        dataType: "json",
        success: function (response) {
            const $select = $("#studentSelect");
            $select.empty();
            $select.append('<option value="">Seleccionar estudiante...</option>');

            if (response.data && response.data.length > 0) {
                response.data.forEach(function (student) {
                    if (student.id != currentCaptainId) {
                        const fullName = `${student.name || ''} ${student.lastName || ''} ${student.secondLastName || ''}`.trim();
                        $select.append(`<option value="${student.id}">${fullName} - ${student.controlNumber || 'Sin No. Control'}</option>`);
                    }
                });
                $("#changeCaptainModal").modal('show');
            } else {
                toastr.warning('No hay estudiantes disponibles para seleccionar como capitán');
            }
        },
        error: function () {
            toastr.error('Error al cargar la lista de estudiantes');
        }
    });
}

function changeCaptain(newCaptainId) {
    const teamId = $("#teamId").val();

    $.ajax({
        url: changeCaptainUrl,
        type: "POST",
        data: {
            teamId: teamId,
            newCaptainId: newCaptainId
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Capitán actualizado exitosamente');
                $("#changeCaptainModal").modal('hide');
                // Reload the page to reflect changes
                location.reload();
            } else {
                toastr.error(response.message || 'Error al cambiar el capitán');
            }
        },
        error: function () {
            toastr.error('Error al procesar la solicitud');
        }
    });
}

function initializeDataTable(teamId) {
    dataTable = $("#tblRoster").DataTable({
        "ajax": {
            "url": `${getStudentsUrl}?teamId=${teamId}`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "imageUrl",
                "render": function (imageUrl) {
                    return `<img src="${imageUrl}" alt="Foto del estudiante" class="img-thumbnail" style="width: 100px;" onerror="this.onerror=null; this.src='/images/zorro_default.png';" />`;
                },
                "width": "5%",
                "responsivePriority": 5
            },
            {
                "data": null,
                "render": function (data) {
                    const name = data.name || 'Sin actualizar';
                    const lastName = data.lastName || 'Sin actualizar';
                    const secondLastName = data.secondLastName || 'Sin actualizar';
                    if (name === 'Sin actualizar' &&
                        lastName === 'Sin actualizar' &&
                        secondLastName === 'Sin actualizar') {
                        return 'Sin actualizar';
                    }
                    return `${name} ${lastName} ${secondLastName}`;
                },
                "width": "15%",
                "responsivePriority": 1
            },
            {
                "data": "career",
                "render": function (data) {
                    return data || 'Sin actualizar';
                },
                "width": "25%",
                "responsivePriority": 2
            },
            {
                "data": "controlNumber",
                "render": function (data) {
                    return data || 'Sin actualizar';
                },
                "width": "10%",
                "responsivePriority": 3
            },
            {
                "data": null,
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center align-items-center">
                            <div class="btn-group gap-2" role="group">
                                <a onclick=Lock("/Admin/Students/Lock/${data.id}") class="btn btn-danger btn-sm" style="cursor:pointer;">
                                    <i class="fas fa-user-minus"></i>
                                </a>
                                <a onclick=downloadInfo("/Admin/Teams/GenerateStudentCertificate/${data.id}") class="btn btn-info btn-sm" style="cursor:pointer;">
                                    <i class="fas fa-download"></i>
                                </a>
                                <button class="btn btn-secondary btn-sm" onclick="descargarDocs(${data.id})">
                                    <i class="fas fa-file-download"></i>
                                </button>
                            </div>
                        </div>
                    `;
                },
                "width": "15%",
                "responsivePriority": 1
            }
        ],
        "language": {
            "decimal": "",
            "emptyTable": "No hay estudiantes registrados en este equipo",
            "info": "Mostrando _START_ a _END_ de _TOTAL_ Estudiantes",
            "infoEmpty": "Mostrando 0 a 0 de 0 Estudiantes",
            "infoFiltered": "(Filtrado de _MAX_ total estudiantes)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ Estudiantes",
            "loadingRecords": "Cargando...",
            "processing": "Procesando...",
            "search": "Buscar:",
            "zeroRecords": "No se encontraron estudiantes",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            }
        },
        "width": "100%",
        "responsive": true,
        "order": [[1, "asc"]]
    });
}

function generarTarjetas(teamId) {
    $.ajax({
        url: `${getStudentsUrl}?teamId=${teamId}`,
        type: "GET",
        dataType: "json",
        success: function (response) {
            const $cardView = $("#cardView");
            $cardView.empty();

            if (response.data && response.data.length > 0) {
                response.data.forEach(function (student) {
                    const imageUrl = student.imageUrl || '/images/no-image.png';
                    const name = student.name || 'Sin actualizar';
                    const lastName = student.lastName || 'Sin actualizar';
                    const secondLastName = student.secondLastName || 'Sin actualizar';
                    const fullName = (name === 'Sin actualizar' &&
                        lastName === 'Sin actualizar' &&
                        secondLastName === 'Sin actualizar')
                        ? 'Sin actualizar'
                        : `${name} ${lastName} ${secondLastName}`;
                    const card = `
                        <div class="col-md-4 col-lg-3 mb-4">
                            <div class="card h-100">
                                <img src="${imageUrl}" 
                                     class="card-img-top" 
                                     alt="Foto estudiante"
                                     style="height: 200px; object-fit: scale-down; background-color: #f8f9fa;"
                                     onerror="this.onerror=null; this.src='/images/zorro_default.png';">
                                <div class="card-body">
                                    <h5 class="card-title">${fullName}</h5>
                                    <p class="card-text">
                                        <strong>No. Control:</strong> ${student.controlNumber || 'Sin actualizar'}<br>
                                        <strong>Carrera:</strong> ${student.career || 'Sin actualizar'}
                                    </p>
                                    <div class="d-flex justify-content-center mt-3">
                                        <div class="btn-group gap-2" role="group">
                                            <a onclick="Lock('/Admin/Students/Lock/${student.id}')" 
                                               class="btn btn-danger btn-sm" 
                                               style="cursor:pointer;"
                                               title="Dar de baja">
                                                <i class="fas fa-user-minus"></i>
                                            </a>
                                            <a onclick="downloadInfo('/Admin/Teams/GenerateStudentCertificate/${student.id}')" 
                                               class="btn btn-info btn-sm" 
                                               style="cursor:pointer;"
                                               title="Descargar cédula">
                                                <i class="fas fa-download"></i>
                                            </a>
                                            <button class="btn btn-secondary btn-sm" 
                                                    onclick="descargarDocs(${student.id})"
                                                    title="Descargar documentos">
                                                <i class="fas fa-file-download"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    $cardView.append(card);
                });
            } else {
                $cardView.append(`
                    <div class="col-12">
                        <div class="alert alert-info">
                            No hay estudiantes registrados en este equipo
                        </div>
                    </div>
                `);
            }
        },
        error: function () {
            $("#cardView").html(`
                <div class="col-12">
                    <div class="alert alert-danger">
                        Error al cargar los estudiantes
                    </div>
                </div>
            `);
        }
    });
}

function Lock(url) {
    const teamId = $("#teamId").val();
    swal({
        title: "¿Está seguro de dar de baja a este estudiante del equipo?",
        text: "¡Este estudiante no se volverá a mostrar hasta que se dé de alta en el equipo!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "¡Sí, dar de baja!",
        closeOnConfirm: true
    }, function () {
        $.ajax({
            type: 'GET',
            url: url,
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    // Verificar la vista actual y recargar según corresponda
                    if ($('#cardView').is(':visible')) {
                        generarTarjetas(teamId);
                    } else {
                        dataTable.ajax.reload();
                    }
                } else {
                    toastr.error(data.message);
                }
            }
        });
    });
}

function downloadInfo(url) {
    $.ajax({
        url: url,
        type: 'POST',
        xhrFields: { responseType: 'blob' }, // Configura la respuesta para manejar blobs
        success: function (blob, status, xhr) {
            // Obtener el nombre del archivo desde el encabezado 'Content-Disposition'
            const disposition = xhr.getResponseHeader('Content-Disposition');
            let fileName = "Informacion_Estudiantes.pdf"; // Nombre por defecto

            if (disposition && disposition.includes('filename=')) {
                const filenameRegex = /filename[^;=\n]*=(['"]?)([^'"\n]*)\1/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[2]) {
                    fileName = matches[2];
                }
            }

            // Crear un enlace temporal para descargar el archivo
            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName; // Usa el nombre del archivo extraído
            link.click();
            window.URL.revokeObjectURL(link.href);

            toastr.success("Documento descargado correctamente.");
        },
        error: function (error) {
            toastr.error("Error al generar el documento de información del estudiante:", error);
        }
    });
}

function downloadAllInfo() {
    const teamId = $("#teamId").val();
    $.ajax({
        url: `${downloadAllInfoUrl}?teamId=${teamId}`,
        type: 'POST',
        xhrFields: { responseType: 'blob' },
        success: function (blob, status, xhr) {
            // Obtener el nombre del archivo desde el encabezado 'Content-Disposition'
            const disposition = xhr.getResponseHeader('Content-Disposition');
            let fileName = "Informacion_Estudiantes.pdf"; // Nombre por defecto

            if (disposition && disposition.includes('filename=')) {
                const filenameRegex = /filename[^;=\n]*=(['"]?)([^'"\n]*)\1/;
                const matches = filenameRegex.exec(disposition);
                if (matches != null && matches[2]) {
                    fileName = matches[2];
                }
            }

            // Crear un enlace temporal para descargar el archivo
            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName; // Usa el nombre del archivo extraído
            link.click();
            window.URL.revokeObjectURL(link.href);

            toastr.success("Documento descargado correctamente.");
        },
        error: function (xhr) {
            if (xhr.status === 404) {
                toastr.error("No se encontraron estudiantes en el equipo");
            } else {
                toastr.error("Error al generar el documento de información de los estudiantes.");
            }
        }
    });
}


function loadStudentsForCertificate() {
    const teamId = $("#teamId").val();
    $.ajax({
        url: `${getStudentsUrl}?teamId=${teamId}`,
        type: 'GET',
        data: { teamId: teamId },
        success: function (response) {
            if (response.data && response.data.length > 0) {
                allStudents = response.data;  // Guardar todos los estudiantes en la variable global
                let studentsHtml = '';
                $(".checkbox-div").show();
                $('.not-found').hide();

                response.data.forEach(student => {
                    const controlNumber = student.controlNumber ? student.controlNumber : '---';
                    const studentImage = student.imageUrl
                        ? `
                                    <img src="${student.imageUrl}" class="card-img-top h-100" alt="Imagen del jugador" />`
                        : `<div class="image-placeholder">
                                        <i class="fa-regular fa-user"></i>
                                    </div>`;

                    studentsHtml += `
                                    <div class="col">
                                        <div class="card h-100 position-relative">
                                            ${studentImage}
                                            <div class="position-absolute top-0 end-0 m-2">
                                                <input type="checkbox" class="btn-check student-checkbox" id="checkbox${student.id}" autocomplete="off">
                                                <label class="btn btn-sm checkbox" for="checkbox${student.id}">
                                                    <i class="fa-solid fa-check"></i>
                                                </label>
                                            </div>
                                            <div class="card-body p-0 text-center">
                                                <h5 class="text-center mb-0 ps-2 pe-2 text-white title fs-6 d-flex align-items-center justify-content-center">
                                                    ${student.name} ${student.lastName} ${student.secondLastName}
                                                </h5>
                                                <p class="mb-0 control-number">${controlNumber}</p>
                                            </div>
                                        </div>
                                    </div>
                                `;
                });

                $('.students-container').html(studentsHtml);
                $("#certificateModal").modal('show');
            } else {
                $('.not-found').show();
                $(".checkbox-div").hide();
                $("#certificateModal").modal('show');
            }
        },
        error: function (error) {
            toastr.error('Error al cargar la lista de estudiantes');
        }
    });

    $('#checkboxAll').change(function () {
        const isChecked = $(this).is(':checked');
        $('.student-checkbox').prop('checked', isChecked).trigger('change');
    });

    $(document).on('change', '.student-checkbox', function () {
        if (!$(this).is(':checked')) {
            $('#checkboxAll').prop('checked', false);
        } else {
            const allChecked = $('.student-checkbox:checked').length === $('.student-checkbox').length;
            $('#checkboxAll').prop('checked', allChecked);
        }
    });
}

function generateCertificate() {
    // Filtra los estudiantes seleccionados a partir de la lista completa almacenada
    const teamId = $("#teamId").val();
    const selectedStudents = allStudents.filter(student =>
        $(`#checkbox${student.id}`).is(':checked')
    );

    if (selectedStudents.length === 0) {
        toastr.error('No hay estudiantes seleccionados');
        return;
    }

    const today = new Date();
    const formattedDate = today.toISOString().split('T')[0];

    // Envía los objetos de estudiantes seleccionados al controlador
    $.ajax({
        url: `${downloadCertificate}?teamId=${teamId}`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            students: selectedStudents,
            team: teamName,
            coach: coachTeam
        }),
        xhrFields: { responseType: 'blob' },
        success: function (blob) {
            const link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = `Cedula_${teamName}_${formattedDate}.pdf`;
            link.click();
            toastr.success('Cedula generada');
        },
        error: function (error) {
            toastr.error('Error al generar la cedula');
        }
    });
} 