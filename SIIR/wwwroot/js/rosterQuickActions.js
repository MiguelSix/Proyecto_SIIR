let dataTable;
let dataTableTarjetas;
let currentCaptainId;
let allStudents = [];

$(document).ready(function () {

    $(document).on("click", "#generateCertificateBtn", function () {
        console.log("Holaaaa");
        const teamId = $(this).data("team-id");
        loadStudentsForCertificate(teamId);
    });

    //$("#btn-generar-cedula").click(function () {
    //    generateCertificate();
    //});
});

function loadStudentsForCertificate(teamId) {
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
                    const defaultImage = '/images/zorro_default.png';
                    const studentImage = student.imageUrl
                        ? `<img src="${student.imageUrl}" class="card-img-top h-100" alt="Imagen del jugador" onerror="this.onerror=null;this.src='${defaultImage}';" />`
                        : `<img src="${defaultImage}" class="card-img-top h-100" alt="Imagen del jugador" />`;

                    studentsHtml += `
        <div class="col">
            <div class="card position-relative">
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

/*function generateCertificate() {
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
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0'); // Mes en formato 'MM'
    const day = String(today.getDate()).padStart(2, '0'); // Día en formato 'DD'
    const formattedDate = `${year}-${month}-${day}`;
    const formattedTeamName = teamName.replace(/\s+/g, '_');

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
            link.download = `Cedula_${formattedTeamName}_${formattedDate}.pdf`;
            link.click();
            toastr.success('Cedula generada');
        },
        error: function (error) {
            toastr.error('Error al generar la cedula');
        }
    });
} */