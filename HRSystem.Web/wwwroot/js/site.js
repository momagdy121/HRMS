/* ========================================
   HRSystem.Web — Site JavaScript
   Modal, Toast, and UI Interaction Helpers
   ======================================== */

// === Modal Functions ===
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// Close modal on overlay click
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay') && e.target.classList.contains('active')) {
        e.target.classList.remove('active');
        document.body.style.overflow = '';
    }
});

// Close modal on Escape key
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        const activeModals = document.querySelectorAll('.modal-overlay.active');
        activeModals.forEach(function (modal) {
            modal.classList.remove('active');
        });
        document.body.style.overflow = '';
    }
});

// === Toast Functions ===
function showToast(toastId, autoDismissMs) {
    const toast = document.getElementById(toastId);
    if (toast) {
        toast.classList.add('active');
        if (autoDismissMs && autoDismissMs > 0) {
            setTimeout(function () {
                hideToast(toastId);
            }, autoDismissMs);
        }
    }
}

function hideToast(toastId) {
    const toast = document.getElementById(toastId);
    if (toast) {
        toast.classList.remove('active');
    }
}

// === Tab/Pill Switcher ===
function switchTab(tabGroupId, activeIndex) {
    const group = document.getElementById(tabGroupId);
    if (!group) return;
    const tabs = group.querySelectorAll('[data-tab-target]');
    const panels = group.querySelectorAll('[data-tab-panel]');

    tabs.forEach(function (tab, i) {
        if (i === activeIndex) {
            tab.classList.add('bg-white', 'shadow-sm', 'text-primary', 'font-semibold', 'border', 'border-slate-200');
            tab.classList.remove('text-slate-600', 'hover:text-slate-900');
        } else {
            tab.classList.remove('bg-white', 'shadow-sm', 'text-primary', 'font-semibold', 'border', 'border-slate-200');
            tab.classList.add('text-slate-600', 'hover:text-slate-900');
        }
    });

    panels.forEach(function (panel, i) {
        panel.style.display = i === activeIndex ? 'block' : 'none';
    });
}

// === Mobile Sidebar Toggle ===
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        sidebar.classList.toggle('hidden');
        sidebar.classList.toggle('md:flex');
    }
}

// === Wizard Step Navigation ===
function goToWizardStep(wizardId, stepIndex) {
    const wizard = document.getElementById(wizardId);
    if (!wizard) return;
    const steps = wizard.querySelectorAll('[data-wizard-step]');
    const panels = wizard.querySelectorAll('[data-wizard-panel]');
    const circles = wizard.querySelectorAll('.wizard-step-circle');

    panels.forEach(function (panel, i) {
        panel.style.display = i === stepIndex ? 'block' : 'none';
    });

    circles.forEach(function (circle, i) {
        circle.classList.remove('active', 'completed', 'inactive');
        if (i < stepIndex) {
            circle.classList.add('completed');
        } else if (i === stepIndex) {
            circle.classList.add('active');
        } else {
            circle.classList.add('inactive');
        }
    });
}
