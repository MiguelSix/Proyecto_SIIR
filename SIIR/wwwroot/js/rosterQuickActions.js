﻿let dataTable;
let dataTableTarjetas;
let currentCaptainId;
let allStudents = [];

$(document).ready(function () {

    $(document).on("click", "#generateCertificateBtn", function () {
        const teamId = $(this).data("team-id");
        loadStudentsForCertificate(teamId);
    });

    $(document).on("click", "#downloadStudentsInfo", function () {
        const teamId = $(this).data("team-id");
        downloadAllInfo(teamId);
    });

});

function loadStudentsForCertificate(teamId) {
    $.ajax({
        url: `${getStudentsUrl}?teamId=${teamId}`,
        type: 'GET',
        data: { teamId: teamId },
        success: function (response) {
            let studentsHtml = '';
            if (response.data && response.data.length > 0) {
                allStudents = response.data;  // Guardar todos los estudiantes en la variable global

                response.data.forEach(student => {
                    const controlNumber = student.controlNumber ? student.controlNumber : '---';
                    const defaultImage = '/images/zorro_default.png';
                    const studentImage = student.imageUrl
                        ? `<img src="${student.imageUrl}" class="card-img-top h-100" alt="Imagen del jugador" onerror="this.onerror=null;this.src='${defaultImage}';" />`
                        : `<img src="${defaultImage}" class="card-img-top h-100" alt="Imagen del jugador" />`;

                    studentsHtml += `
                        <div class="col">
                            <div class="card position-relative">
                                ${studentImage}
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
                $('.not-found-container').empty();
                $('.students-container').html(studentsHtml);
                $("#certificateModal").modal('show');
            } else {
                studentsHtml += `
                        <div class="not-found text-center mt-4">
                            <i class="fa-solid fa-ban fs-2" style="color: red"></i>
                            <p class="fs-2 mb-0">No se encontraron elementos</p>
                        </div>
                    `;
                $('.students-container').empty();
                $('.not-found-container').html(studentsHtml);
                $("#certificateModal").modal('show');
            }
        },
        error: function (error) {
            toastr.error('Error al cargar la lista de estudiantes');
        }
    });
}

function downloadAllInfo(teamId) {
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