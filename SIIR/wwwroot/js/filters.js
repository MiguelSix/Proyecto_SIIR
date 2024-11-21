$(document).ready(function () {
    setupMultiSelect('careerFilter', 'Carreras');
    setupMultiSelect('semesterFilter', 'Semestres');
    const $fullWidthContainer = $('<div/>', {
        class: 'col-12'
    });

    $('<div/>', {
        id: 'activeFilters',
        class: 'active-filters mt-3'
    }).appendTo($fullWidthContainer);

    $('.row:eq(1)').append($fullWidthContainer);
});

function setupMultiSelect(selectId, label) {
    const $originalSelect = $(`#${selectId}`);
    const $container = $('<div/>', {
        class: 'multi-select-container'
    });

    const $button = $('<button/>', {
        class: 'multi-select-button',
        html: `<span>${label}</span>
               <i class="fas fa-chevron-down ms-2"></i>`
    });

    const $dropdown = $('<div/>', {
        class: 'multi-select-dropdown'
    });

    const $optionsList = $('<div/>', {
        class: 'multi-select-options'
    });

    $originalSelect.find('option').each(function () {
        if ($(this).val()) { // Ignorar la opción vacía
            const $option = $('<label/>', {
                class: 'multi-select-option',
                html: `<input type="checkbox" value="${$(this).val()}"> 
                       <span>${$(this).text()}</span>`
            });
            $optionsList.append($option);
        }
    });

    $dropdown.append($optionsList);
    $container.append($button).append($dropdown);
    $originalSelect.hide().after($container);

    $button.on('click', function (e) {
        e.stopPropagation();
        $dropdown.toggleClass('show');
        $button.toggleClass('active');
    });

    $optionsList.on('change', 'input[type="checkbox"]', function () {
        updateActiveFilters();
        if (window.dataTable) {
            window.dataTable.ajax.reload();
        }
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.multi-select-container').length) {
            $('.multi-select-dropdown').removeClass('show');
            $('.multi-select-button').removeClass('active');
        }
    });
}

function updateActiveFilters() {
    const $activeFilters = $('#activeFilters');
    $activeFilters.empty();

    const selectedFilters = {
        careers: $('.multi-select-container:eq(0) input:checked').map(function () {
            return $(this).val();
        }).get(),
        semesters: $('.multi-select-container:eq(1) input:checked').map(function () {
            return $(this).val();
        }).get()
    };

    selectedFilters.careers.forEach(career => {
        createFilterBubble(career, 'career', $activeFilters);
    });

    selectedFilters.semesters.forEach(semester => {
        createFilterBubble(semester, 'semester', $activeFilters);
    });

    window.selectedFilters = selectedFilters;
}
function createFilterBubble(text, type, container) {
    let displayText = text;
    if (type === 'semester' && text !== 'Egresado') {
        displayText = `${text} semestre`;
    }

    const $bubble = $('<div/>', {
        class: `filter-bubble ${type}-bubble`,
        html: `
            <span>${displayText}</span>
            <button class="remove-filter" data-type="${type}" data-value="${text}">
                <i class="fas fa-times"></i>
            </button>
        `
    });

    $bubble.find('.remove-filter').on('click', function () {
        const type = $(this).data('type');
        const value = $(this).data('value');
        $(`.multi-select-container:eq(${type === 'career' ? 0 : 1}) input[value="${value}"]`)
            .prop('checked', false);
        updateActiveFilters();
        if (window.dataTable) {
            window.dataTable.ajax.reload();
        }
    });

    container.append($bubble);
}