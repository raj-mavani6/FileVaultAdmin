// FileVault Admin — Global Scripts

// Theme toggle
(function () {
    const saved = localStorage.getItem('fva-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
})();

function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('fva-theme', next);
    const icon = document.querySelector('#themeToggle i');
    if (icon) icon.className = next === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
}

// Toast
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const icons = { 
        success: 'bi-check-circle-fill', 
        danger: 'bi-exclamation-triangle-fill', 
        info: 'bi-info-circle-fill', 
        warning: 'bi-exclamation-circle-fill' 
    };
    
    const colors = { 
        success: '#2ecc71', 
        danger: '#ff4757', 
        info: '#3742fa', 
        warning: '#ffa502' 
    };

    const toast = document.createElement('div');
    toast.className = 'toast adm-toast show';
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="toast-body d-flex align-items-center gap-3">
            <div class="adm-toast-icon-wrapper" style="color:${colors[type] || colors.info};">
                <i class="bi ${icons[type] || icons.info}" style="font-size:1.4rem;"></i>
            </div>
            <div class="flex-grow-1">
                <span class="fw-600" style="color: var(--adm-text); font-size: 0.9rem;">${message}</span>
            </div>
            <button type="button" class="btn-close" onclick="this.closest('.toast').remove()"></button>
        </div>
    `;
    container.appendChild(toast);
    setTimeout(() => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 400);
    }, 5000);
}

// Scroll animations
document.addEventListener('DOMContentLoaded', () => {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => entry.target.classList.add('visible'), index * 60);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -30px 0px' });
    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));

    // Update theme icon on load
    const theme = document.documentElement.getAttribute('data-theme');
    const icon = document.querySelector('#themeToggle i');
    if (icon) icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
});
