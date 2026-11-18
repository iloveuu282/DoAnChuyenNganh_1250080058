document.querySelectorAll('form[data-confirm]').forEach(form=>form.addEventListener('submit',event=>{if(!confirm(form.dataset.confirm))event.preventDefault();}));
const type=document.querySelector('[data-flow]');
function filterCategories(){if(!type)return;const select=document.querySelector('[data-categories]');if(!select)return;for(const option of select.options){if(option.dataset.type){option.hidden=option.dataset.type!==type.value;option.disabled=option.hidden;}}if(select.selectedOptions[0]?.disabled)select.value='';}
if(type){type.addEventListener('change',filterCategories);filterCategories();}
// Đánh dấu trang hiện tại bằng thuộc tính; màu lấy từ CSS chung.
document.querySelectorAll('nav a').forEach(link => {
    const target = new URL(link.href).pathname.toLowerCase().replace(/\/$/, '') || '/';
    const current = location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    const active = target === '/' ? current === '/' || current === '/home' || current === '/home/index'
        : current === target || current.startsWith(target + '/');
    if (active) link.setAttribute('aria-current', 'page');
});
