
    divs = document.querySelectorAll('div[title="Дедлайн провален"]');

    divs2 = document.querySelectorAll('div[title="Дедлайн близок к провалу"]');

divs.forEach(function (item, i, divs) {
    item.classList.remove('border-info');
    item.classList.add('border-danger');
});

divs2.forEach(function (item, i, divs2) {
    item.classList.remove('border-info');
    item.classList.add('border-warning');
});