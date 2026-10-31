const headerBtn = document.querySelector("header .menu-btn");
const headerMenu = document.querySelector("header .menu");
const headerClose = document.querySelector("header .menu .menu-head svg");


headerBtn.addEventListener("click", function () {
    headerMenu.classList.toggle("active")
})

headerClose.addEventListener("click", function () {
    headerMenu.classList.remove("active")
})


const dropdownButtons = document.querySelectorAll(".sidebar-dropdown-btn");

dropdownButtons.forEach(button => {
    button.addEventListener("click", () => {

        const menu = button.parentElement.querySelector(".sidebar-dropdown-menu");

        button.classList.toggle("active");
        menu.classList.toggle("open");

    });
});