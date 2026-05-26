% calculate shifts, etc.

addpath '/Users/yookyunglee/MIT Dropbox/Yoo Lee/Cold Atoms/Lab data/_matlab_niceFunctions'

load('wannier_results_li_2026','wannier_results')
B_prep = 875.53; % B field in G where bc rf flips happen 

% Physical constants for Li 7
mass            = 1.443e-25*7/87;
a_lattice       = 532e-9;
k_lattice       = 2*pi/(2*a_lattice);
h               = 6.626e-34;
hbar            = h / (2*pi);
ER              = hbar^2* k_lattice^2 / (2*mass);
joules_2_Hz     = 1/h;
a0              = 5.29e-11; % Bohr radius

Uintegral0 = interp1(wannier_results.Vlat,wannier_results.Uintegral,37,'cubic');

RF_shift_bb_bc_kHz = Uintegral0 * (fitbb(B_prep)-fitbc(B_prep))  * (a0 / a_lattice) * (ER / h / 1000)