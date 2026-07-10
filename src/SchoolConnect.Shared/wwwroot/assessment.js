window.schoolConnectAssessment = {
    getSelections: function () {
        return Array.from(document.querySelectorAll('.assessment-question-card input[type="radio"]:checked'))
            .map(function (input) {
                return (input.dataset.questionNumber || '0') + '\t' + (input.value || '');
            })
            .filter(function (selection) {
                return Number.parseInt(selection.split('\t', 1)[0], 10) > 0;
            })
            .join('\n');
    }
};
