$(document).ready(function () {
    cargarEquipos();
});

function cargarEquipos() {
    $.ajax({
        url: '/Coach/Home/GetAllTeams',
        type: 'GET',
        success: function (data) {
            var teamsContainer = $("#teamsContainer");
            teamsContainer.empty(); // Limpiar el contenedor antes de agregar las tarjetas

            // Iterar sobre los equipos obtenidos y crear las tarjetas
            data.data.forEach(function (team) {
                // Obtener las categorías del equipo y del representative
                var teamCategory = team.category ? team.category.toLowerCase() : 'sin categoría';
                var representativeCategory = team.representative && team.representative.category
                    ? team.representative.category.toLowerCase()
                    : 'sin categoría';

                // Determinar el color de la cápsula según la categoría del equipo (femenil/varonil)
                var teamCategoryClass = 'bg-secondary'; // Valor por defecto para equipos sin categoría
                switch (teamCategory) {
                    case 'femenino':
                        teamCategoryClass = 'bg-danger';
                        break;
                    case 'masculino':
                        teamCategoryClass = 'bg-primary';
                        break;
                }

                // Determinar el color de la cápsula según la categoría del representative (cultural/deportivo)
                var representativeCategoryClass = 'bg-secondary'; // Valor por defecto para representative sin categoría
                switch (representativeCategory) {
                    case 'deportivo':
                        representativeCategoryClass = 'bg-success';
                        break;
                    case 'cultural':
                        representativeCategoryClass = 'bg-warning';
                        break;
                }

                // Construir las cápsulas de categoría si ambas existen
                var categoryBadges = `
                    <span class="badge rounded-pill ${teamCategoryClass}">${team.category}</span>
                    <span class="badge rounded-pill ${representativeCategoryClass}">${team.representative.category}</span>
                `;

                var cardHtml = `
                    <div class="col">
                        <div class="card h-100">
                            <img src="${team.imageUrl}" class="card-img-top h-100" alt="Imagen del equipo" />
                            <p class="text-center fs-6 py-2 mb-0 text-white title">${team.name.toUpperCase()}</p>
                            <div class="card-body">
                                <div class="d-flex flex-wrap gap-2 mb-2">
                                    ${categoryBadges}
                                </div>
                                <div class="d-flex justify-content-end fs-5">
                                    <i class="fa-solid fa-eye me-3" style="color: #1a3c6e"></i>
                                    <i class="fa-solid fa-file-arrow-down" style="color: #2e8b57"></i>
                                </div>
                            </div>
                        </div>
                    </div>`;

                teamsContainer.append(cardHtml);
            });
        },
        error: function (error) {
            console.log("Error al cargar los equipos:", error);
        }
    });
}
