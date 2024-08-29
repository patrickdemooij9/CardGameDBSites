/******/ (() => { // webpackBootstrap
var __webpack_exports__ = {};
/*!**********************!*\
  !*** ./js/banner.ts ***!
  \**********************/
var observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
        if (entry.isIntersecting) {
            observer.unobserve(entry.target);
            loadBanner(entry.target);
        }
    });
}, {
    threshold: [0.5]
});
document.querySelectorAll("[data-banner]").forEach(function (element) {
    observer.observe(element);
});
function loadBanner(element) {
    fetch("/umbraco/api/banner/displaybanner")
        .then(function (resp) { return resp.json(); })
        .then(function (resp) { return element.innerHTML = resp.value; });
}

/******/ })()
;
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiYmFubmVyLWJ1bmRsZS5qcyIsIm1hcHBpbmdzIjoiOzs7OztBQUFBLElBQU0sUUFBUSxHQUFHLElBQUksb0JBQW9CLENBQUMsVUFBQyxPQUFvQztJQUMzRSxPQUFPLENBQUMsT0FBTyxDQUFDLGVBQUs7UUFDakIsSUFBSSxLQUFLLENBQUMsY0FBYyxFQUFDO1lBQ3JCLFFBQVEsQ0FBQyxTQUFTLENBQUMsS0FBSyxDQUFDLE1BQU0sQ0FBQyxDQUFDO1lBRWpDLFVBQVUsQ0FBQyxLQUFLLENBQUMsTUFBTSxDQUFDLENBQUM7U0FDNUI7SUFDTCxDQUFDLENBQUM7QUFDTixDQUFDLEVBQUU7SUFDQyxTQUFTLEVBQUUsQ0FBQyxHQUFHLENBQUM7Q0FDbkIsQ0FBQyxDQUFDO0FBRUgsUUFBUSxDQUFDLGdCQUFnQixDQUFDLGVBQWUsQ0FBQyxDQUFDLE9BQU8sQ0FBQyxVQUFDLE9BQW9CO0lBQ3BFLFFBQVEsQ0FBQyxPQUFPLENBQUMsT0FBTyxDQUFDLENBQUM7QUFDOUIsQ0FBQyxDQUFDLENBQUM7QUFFSCxTQUFTLFVBQVUsQ0FBQyxPQUFnQjtJQUNoQyxLQUFLLENBQUMsbUNBQW1DLENBQUM7U0FDckMsSUFBSSxDQUFDLFVBQUMsSUFBSSxJQUFLLFdBQUksQ0FBQyxJQUFJLEVBQUUsRUFBWCxDQUFXLENBQUM7U0FDM0IsSUFBSSxDQUFDLFVBQUMsSUFBSSxJQUFLLGNBQU8sQ0FBQyxTQUFTLEdBQUcsSUFBSSxDQUFDLEtBQUssRUFBOUIsQ0FBOEIsQ0FBQyxDQUFDO0FBQ3hELENBQUMiLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vanMvYmFubmVyLnRzIl0sInNvdXJjZXNDb250ZW50IjpbImNvbnN0IG9ic2VydmVyID0gbmV3IEludGVyc2VjdGlvbk9ic2VydmVyKChlbnRyaWVzOiBJbnRlcnNlY3Rpb25PYnNlcnZlckVudHJ5W10pID0+IHtcclxuICAgIGVudHJpZXMuZm9yRWFjaChlbnRyeSA9PiB7XHJcbiAgICAgICAgaWYgKGVudHJ5LmlzSW50ZXJzZWN0aW5nKXtcclxuICAgICAgICAgICAgb2JzZXJ2ZXIudW5vYnNlcnZlKGVudHJ5LnRhcmdldCk7XHJcblxyXG4gICAgICAgICAgICBsb2FkQmFubmVyKGVudHJ5LnRhcmdldCk7XHJcbiAgICAgICAgfVxyXG4gICAgfSlcclxufSwge1xyXG4gICAgdGhyZXNob2xkOiBbMC41XVxyXG59KTtcclxuXHJcbmRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCJbZGF0YS1iYW5uZXJdXCIpLmZvckVhY2goKGVsZW1lbnQ6IEhUTUxFbGVtZW50KSA9PiB7XHJcbiAgICBvYnNlcnZlci5vYnNlcnZlKGVsZW1lbnQpO1xyXG59KTtcclxuXHJcbmZ1bmN0aW9uIGxvYWRCYW5uZXIoZWxlbWVudDogRWxlbWVudCl7XHJcbiAgICBmZXRjaChcIi91bWJyYWNvL2FwaS9iYW5uZXIvZGlzcGxheWJhbm5lclwiKVxyXG4gICAgICAgIC50aGVuKChyZXNwKSA9PiByZXNwLmpzb24oKSlcclxuICAgICAgICAudGhlbigocmVzcCkgPT4gZWxlbWVudC5pbm5lckhUTUwgPSByZXNwLnZhbHVlKTtcclxufSJdLCJuYW1lcyI6W10sInNvdXJjZVJvb3QiOiIifQ==