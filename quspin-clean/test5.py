import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp
from scipy.optimize import curve_fit

# =============================================================
# RUN OPTIONS
# =============================================================
SHOW_DIAGNOSTIC = False    # True -> show J(t)/V(t) waveform + harmonic-content panel
                          # False -> skip the diagnostic figure entirely

# =============================================================
# INITIAL AND TARGET FOCK STATES
# =============================================================
state_init   = [1, 0]
state_target = [0, 1]

state_init   = list(state_init)
state_target = list(state_target)
if len(state_init) != len(state_target):
    raise ValueError("state_init and state_target must have the same length (L): "
                     "got %d and %d" % (len(state_init), len(state_target)))
L = len(state_init)
N = sum(state_init)
if sum(state_target) != N:
    raise ValueError("state_init and state_target must have the same particle "
                     "number N. init has N=%d, target has N=%d." % (N, sum(state_target)))

# =============================================================
# LATTICE DEPTH -> TUNNELING CONVERTER
# =============================================================
J_OUTPUT_UNITS = "Hz"
E_R_HZ = 1.0    # recoil energy in Hz; only used if J_OUTPUT_UNITS == "Er"

def V_to_J(V):
    """
    Lattice depth -> tunneling strength.
    From MATLAB plot_wannier_results.m
    Double-exponential fit: < 1% fractional error in J when J < 10 E_r.
    Input V in Er, output in Hz.
        fexp(x) = a*exp(b*x) + c*exp(d*x)
        a = 284.4, b = -0.2922, c = 30.69, d = -0.1304
    Accepts scalar or array V.
    """
    V = np.asarray(V, dtype=float)
    a = 284.4
    b = -0.2922
    c = 30.69
    d = -0.1304
    Jr = a * np.exp(b * V) + c * np.exp(d * V)
    return Jr

# --- static lattice depth ----
V0 = 0.75 * 0.8 * 10.0           # static lattice depth (Er). Our Rabi oscillations don't agree with this setup.
# Our lattice depth was calibrated a long time ago, maybe there is another uncertainty here? It's unclear...

# =============================================================
# Convert a V_to_J output to ANGULAR frequency (rad/s) consistently
# =============================================================
twopi = 2.0 * np.pi
def J_angular(V):
    """Return tunneling as angular frequency (rad/s) for depth V, any input shape."""
    Jval = V_to_J(V)
    if J_OUTPUT_UNITS == "Hz":
        return twopi * Jval
    elif J_OUTPUT_UNITS == "angular":
        return Jval
    elif J_OUTPUT_UNITS == "Er":
        return twopi * E_R_HZ * Jval
    else:
        raise ValueError("J_OUTPUT_UNITS must be 'Hz', 'angular', or 'Er'")

# -----------------------------
# Other physical parameters
# -----------------------------
U_hz     = 285.0
Delta_hz = 835.0

# --- MULTI-TONE DEPTH MODULATION ---------------------------------------------
# V(t) = V0 * (1 + sum_k alpha[k] * cos(omega[k] * t))
alpha        = [0.2]
if np.sum(state_init)==1:
    f_drive_list = [Delta_hz]
elif np.sum(state_init)==2:
    f_drive_list = [Delta_hz + U_hz]
# -----------------------------------------------------------------------------
alpha        = np.atleast_1d(np.asarray(alpha, dtype=float))
f_drive_list = np.atleast_1d(np.asarray(f_drive_list, dtype=float))
if alpha.shape != f_drive_list.shape:
    raise ValueError("alpha and f_drive_list must have the same length.")

U     = twopi * U_hz
Delta = twopi * Delta_hz
omega = twopi * f_drive_list

N_PERIODS_PER_RABI = 100
f_ref_hz = f_drive_list.min()

# -----------------------------
# Fixed-N basis
# -----------------------------
def generate_basis(L, N):
    return [tuple(occ) for occ in itertools.product(range(N + 1), repeat=L)
            if sum(occ) == N]
basis = generate_basis(L, N)
index = {s: i for i, s in enumerate(basis)}
dim   = len(basis)
if tuple(state_init) not in index or tuple(state_target) not in index:
    raise ValueError("init/target state not present in the N=%d basis." % N)

print("L = %d, N = %d, dim = %d" % (L, N, dim))
print("init = %s, target = %s" % (tuple(state_init), tuple(state_target)))
print("static depth V0 = %.4g Er -> J(V0) = %.4f Hz" % (V0, J_angular(V0)/twopi))

# -----------------------------
# Hamiltonian pieces. H_hop carries J=1; we scale it by J_angular(V(t)) at runtime.
# -----------------------------
def build_diag():
    H = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        n = np.array(state)
        H[a, a] = 0.5 * U * np.sum(n*(n-1)) + Delta * np.sum(np.arange(L)*n)
    return H

def build_hop_unit():
    H = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        n = np.array(state)
        for j in range(L-1):
            if n[j] > 0:
                new = n.copy(); amp = np.sqrt(n[j])*np.sqrt(n[j+1]+1)
                new[j]-=1; new[j+1]+=1
                H[index[tuple(new)], a] += -amp
            if n[j+1] > 0:
                new = n.copy(); amp = np.sqrt(n[j+1])*np.sqrt(n[j]+1)
                new[j+1]-=1; new[j]+=1
                H[index[tuple(new)], a] += -amp
    return H

H_diag    = build_diag()
H_hop_unit = build_hop_unit()   # hopping with unit J; multiply by J_angular(V(t))

# -----------------------------
# State and projector
# -----------------------------
psi0 = np.zeros(dim, dtype=complex)
psi0[index[tuple(state_init)]] = 1.0
P_target = np.zeros((dim, dim), dtype=complex)
P_target[index[tuple(state_target)], index[tuple(state_target)]] = 1.0

# -----------------------------
# Depth modulation and evolution
# -----------------------------
def V_of_t(t):
    return V0 * (1.0 + np.sum(alpha * np.cos(omega * t)))

def evolve(n_periods, samples_per_period):
    T_ref = 1.0 / f_ref_hz
    def rhs(t, psi):
        Jt = J_angular(V_of_t(t))            # nonlinear V->J applied here
        return -1j * (H_diag + Jt * H_hop_unit).dot(psi)
    t_end = n_periods * T_ref
    times = np.linspace(0.0, t_end, n_periods * samples_per_period + 1)
    sol = solve_ivp(rhs, (0.0, t_end), psi0, t_eval=times,
                    rtol=1e-9, atol=1e-9, method="RK45")
    return times, sol.y.T

# =============================================================
# TIME TRACE
# =============================================================
n_periods  = 3 * N_PERIODS_PER_RABI
samples_per_period = 40
times, psis = evolve(n_periods, samples_per_period)
print("final norm =", np.linalg.norm(psis[-1]))

# report the effective J swing produced by this depth modulation (diagnostic)
J_swing = np.array([J_angular(V_of_t(t))/twopi for t in
                    np.linspace(0, 1.0/f_ref_hz, 200)])
J0_hz = J_angular(V0)/twopi
print("J over one drive period: min=%.3f  max=%.3f  mean=%.3f Hz (static J0=%.3f)"
      % (J_swing.min(), J_swing.max(), J_swing.mean(), J0_hz))
print("  -> effective fractional J-modulation (peak): alpha'_eff ~ %.3f"
      % ((J_swing.max() - J_swing.min()) / (2.0*J0_hz)))

ptar = np.array([np.vdot(p, P_target.dot(p)).real for p in psis])

w = samples_per_period
strobe = slice(0, len(times), w)
kernel = np.ones(w) / w
ptar_avg = np.convolve(ptar, kernel, mode="valid")
t_avg    = times[(w-1)//2 : (w-1)//2 + len(ptar_avg)]

# -----------------------------
# Rabi fit
# -----------------------------
def rabi_model(t, A, T_rabi, offset):
    return offset + 0.5 * A * (1.0 - np.cos(2.0 * np.pi * t / T_rabi))
sig = ptar_avg - ptar_avg.mean()
dt  = t_avg[1] - t_avg[0]
freqs_fft = np.fft.rfftfreq(len(sig), d=dt)
power = np.abs(np.fft.rfft(sig)); power[0] = 0.0
f_dom = freqs_fft[np.argmax(power)]
T_guess = 1.0 / f_dom if f_dom > 0 else (t_avg[-1] - t_avg[0])
p0 = [ptar_avg.max() - ptar_avg.min(), T_guess, ptar_avg.min()]
try:
    popt, pcov = curve_fit(rabi_model, t_avg, ptar_avg, p0=p0, maxfev=20000)
    A_fit, T_fit, off_fit = popt
    T_err = np.sqrt(np.diag(pcov))[1]
    fit_ok = True
    print("Fitted Rabi period  T_Rabi = %.4f ms (+/- %.4f ms)" % (1000*T_fit, 1000*T_err))
    print("Effective Rabi freq Omega_eff/2pi = %.2f Hz" % (1.0/T_fit))
    print("Fitted amplitude    A = %.3f" % A_fit)
except RuntimeError:
    fit_ok = False
    print("Rabi fit did not converge - try a longer run.")

# -----------------------------
# helpers for labels
# -----------------------------
def ket(s): return r"|%s\rangle" % ",".join(str(x) for x in s)
tones_str = ", ".join("%.0f" % f for f in f_drive_list)
amp_str   = ", ".join("%.2f" % a for a in alpha)

# =============================================================
# DIAGNOSTIC PANEL (gated by SHOW_DIAGNOSTIC)
#   - J(t) AND V(t) over one Floquet period
#   - harmonic content of J(t)
# =============================================================
if SHOW_DIAGNOSTIC:
    # Sample over ONE reference period on a fine, uniform grid.
    T_ref = 1.0 / f_ref_hz
    n_diag = 4096
    t_diag = np.linspace(0.0, T_ref, n_diag, endpoint=False)
    Jt_hz  = np.array([J_angular(V_of_t(t)) / twopi for t in t_diag])   # J(t) in Hz
    Vt     = np.array([V_of_t(t) for t in t_diag])                     # depth V(t) in Er

    J0_diag = J_angular(V0) / twopi
    Jt_mean = Jt_hz.mean()

    # Linear estimate of the J-modulation amplitude per tone (half peak-to-peak)
    Jlin = np.zeros(len(alpha))
    for k in range(len(alpha)):
        Jlin[k] = 0.5 * (V_to_J(V0 * (1 - alpha[k])) - V_to_J(V0 * (1 + alpha[k])))

    # FFT of J(t): harmonic order = freq / f_ref.
    J_fft   = np.fft.rfft(Jt_hz - Jt_mean)
    J_freq  = np.fft.rfftfreq(n_diag, d=t_diag[1]-t_diag[0])
    J_amp   = (2.0 / n_diag) * np.abs(J_fft)
    harm_order = J_freq / f_ref_hz

    print("-------------------------------------")
    print("Jeff estimates (Hz):", ", ".join("%.4g" % J for J in Jlin))
    print("Harmonic content of J(t) (drive at f_ref = %.1f Hz):" % f_ref_hz)
    print("  DC (mean J)      : %8.3f Hz" % Jt_mean)
    for k in range(1, 7):
        idx = np.argmin(np.abs(harm_order - k))
        print("  harmonic %d (%5.0f Hz): %8.4f Hz amplitude" % (k, J_freq[idx], J_amp[idx]))

    # ---- figure: 3 panels ----
    fig, (axV, axJ, axF) = plt.subplots(1, 3, figsize=(16, 4.5))

    # left: lattice depth V(t) over one period
    axV.plot(1e3*t_diag, Vt, color="C4", lw=2)
    axV.axhline(V0, color="k", ls=":", lw=1, label=r"static $V_0$ = %.2f $E_r$" % V0)
    axV.set_xlabel("time within one drive period (ms)")
    axV.set_ylabel(r"lattice depth $V(t)$ ($E_r$)")
    axV.set_title("Lattice depth modulation")
    axV.legend(fontsize=8)

    # middle: J(t) over one period, with static J0 and time-mean marked
    axJ.plot(1e3*t_diag, Jt_hz, color="C0", lw=2)
    axJ.axhline(J0_diag, color="k", ls=":",  lw=1,
                label=r"static $J(V_0)$ = %.1f Hz" % J0_diag)
    axJ.axhline(Jt_mean, color="C3", ls="--", lw=1,
                label=r"time-mean $\bar J$ = %.1f Hz" % Jt_mean)
    axJ.set_xlabel("time within one drive period (ms)")
    axJ.set_ylabel("tunneling $J(t)$ (Hz)")
    axJ.set_title("Tunneling waveform from depth modulation")
    axJ.legend(fontsize=8)

    # right: harmonic spectrum of J(t)
    sel = (harm_order > 0.5) & (harm_order < 8.5)
    axF.stem(harm_order[sel], J_amp[sel], basefmt=" ")
    axF.set_xlabel(r"harmonic order ($f / f_\mathrm{drive}$)")
    axF.set_ylabel("amplitude in $J(t)$ (Hz)")
    axF.set_title("Harmonic content of J(t)")
    axF.set_xticks(range(1, 9))

    plt.tight_layout()
    plt.savefig("test5_Jdiagnostic.png", dpi=120)
    plt.show()

# -----------------------------
# Main plot: projection onto target vs time
# -----------------------------
plt.figure(figsize=(9, 5))
plt.plot(1000*times, ptar, color="C2", alpha=0.3, lw=0.8, label="raw")
plt.plot(1000*times[strobe], ptar[strobe], "o", color="C2", ms=3, label="stroboscopic")
plt.plot(1000*t_avg, ptar_avg, "-", color="C3", lw=2, label="Floquet-averaged")
if fit_ok:
    plt.plot(1000*t_avg, rabi_model(t_avg, *popt), "k--", lw=1.5,
             label=r"fit: $T_\mathrm{Rabi}=%.3f$ ms, $f_\mathrm{Rabi}=%.1f$ Hz"
                   % (1000*T_fit, 1.0/T_fit))
plt.xlabel("time (ms)")
plt.ylabel(r"projection onto $%s$" % ket(state_target))
plt.title(r"$%s \to %s$;  depth mod $\alpha$=[%s] @ [%s] Hz"
          % (ket(state_init), ket(state_target), amp_str, tones_str))
plt.ylim(0, 1.05)
plt.legend()
plt.tight_layout()
plt.savefig("test5_output.png", dpi=120)
plt.show()