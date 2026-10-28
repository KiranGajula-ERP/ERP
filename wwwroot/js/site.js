document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('[data-confirm]').forEach(el => el.addEventListener('click', ev => {
    if (!confirm(el.getAttribute('data-confirm') || 'Are you sure?')) ev.preventDefault();
  }));
  document.querySelectorAll('[data-project-filter]').forEach(select => select.addEventListener('change', () => {
    const url = new URL(window.location.href); url.searchParams.set('projectId', select.value); window.location.href = url.toString();
  }));
  document.querySelectorAll('[data-filter-input]').forEach(input => input.addEventListener('keydown', ev => {
    if (ev.key === 'Enter') { ev.preventDefault(); input.form?.requestSubmit(); }
  }));
});
