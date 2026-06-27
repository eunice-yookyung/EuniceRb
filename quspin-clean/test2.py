import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp

# =============================================================
# CHOOSE WHAT TO RUN
# =============================================================
MODE = "time"          # "time"  -> population in |0,2> vs time at one frequency
                       # "scan"  -> peak doublon transfer vs drive frequency

# -----------------------------
# Parameters
# -----------------------------
L = 2
N = 2

J_hz     = 20.0
U_hz     = 280.0
Delta_hz = 825.0
alpha    = 1.0

twopi = 2.0 * np.pi
J     = twopi * J_hz
U     = twopi * U_hz
Delta = twopi * Delta_hz

f_res_hz = Delta_hz + U_hz          # expected resonance: |1,1> <-> |0,2>
# f_res_hz = Delta_hz          # expected resonance: |1,1> <-> |0,2>

# n_periods ~ f_drive / Omega_eff  = number of drive cycles per Rabi period
N_PERIODS_PER_RABI = 100

# -----------------------------
# Basis + operators (built once, shared by both modes)
# -----------------------------
def generate_basis(L, N):
    return [tuple(occ) for occ in itertools.product(range(N + 1), repeat=L)
            if sum(occ) == N]

basis = generate_basis(L, N)        # (2,0), (1,1), (0,2)
index = {s: i for i, s in enumerate(basis)}
dim   = len(basis)

def build_diag():
    H = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        n = np.array(state)
        H[a, a] = 0.5 * U * np.sum(n*(n-1)) + Delta * np.sum(np.arange(L)*n)
    return H

def build_hop():
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

H_diag = build_diag()
H_hop  = build_hop()

# doublon operator (=1 in |2,0> and |0,2>, =0 in |1,1>)
D_op = np.zeros((dim, dim), dtype=complex)
for a, state in enumerate(basis):
    n = np.array(state)
    D_op[a, a] = 0.5 * np.sum(n*(n-1))

# projector onto |0,2> specifically (population in that Fock state)
P02 = np.zeros((dim, dim), dtype=complex)
P02[index[(0, 2)], index[(0, 2)]] = 1.0
# P02[index[(0, 1)], index[(0, 1)]] = 1.0

psi0 = np.zeros(dim, dtype=complex)
psi0[index[(1, 1)]] = 1.0
# psi0[index[(1, 0)]] = 1.0

# -----------------------------
# Core evolution routine (shared)
# -----------------------------
def evolve(f_drive_hz, n_periods, samples_per_period):
    """Return (times, psis) for a drive at f_drive_hz."""
    omega = twopi * f_drive_hz
    T = 1.0 / f_drive_hz

    def rhs(t, psi):
        J_t = J * (1.0 + alpha * np.cos(omega * t))
        return -1j * (H_diag + J_t * H_hop).dot(psi)

    t_end = n_periods * T
    times = np.linspace(0.0, t_end, n_periods * samples_per_period + 1)
    sol = solve_ivp(rhs, (0.0, t_end), psi0, t_eval=times,
                    rtol=1e-9, atol=1e-9, method="RK45")
    return times, sol.y.T

from scipy.optimize import curve_fit

# =============================================================
# MODE 1: TIME TRACE  — population in |0,2> vs time
# =============================================================
if MODE == "time":
    f_drive_hz = f_res_hz
    n_periods  = 3 * N_PERIODS_PER_RABI
    samples_per_period = 40

    times, psis = evolve(f_drive_hz, n_periods, samples_per_period)
    print("final norm =", np.linalg.norm(psis[-1]))

    p02 = np.array([np.vdot(p, P02.dot(p)).real for p in psis])

    # stroboscopic + floquet-averaged versions of the same signal
    w = samples_per_period
    strobe = slice(0, len(times), w)
    kernel = np.ones(w) / w
    p02_avg = np.convolve(p02, kernel, mode="valid")
    t_avg   = times[(w-1)//2 : (w-1)//2 + len(p02_avg)]

    # -----------------------------
    # Fit Rabi period to the Floquet-averaged envelope
    #   model: P(t) = offset + 0.5*A*(1 - cos(2*pi*t/T_Rabi))
    # -----------------------------
    def rabi_model(t, A, T_rabi, offset):
        return offset + 0.5 * A * (1.0 - np.cos(2.0 * np.pi * t / T_rabi))

    # --- seed T_Rabi from an FFT of the (mean-subtracted) averaged signal ---
    sig = p02_avg - p02_avg.mean()
    dt  = t_avg[1] - t_avg[0]
    freqs_fft = np.fft.rfftfreq(len(sig), d=dt)
    power = np.abs(np.fft.rfft(sig))
    power[0] = 0.0                      # ignore DC
    f_dom = freqs_fft[np.argmax(power)] # dominant frequency of the envelope
    T_guess = 1.0 / f_dom if f_dom > 0 else (t_avg[-1] - t_avg[0])

    p0 = [p02_avg.max() - p02_avg.min(),     # amplitude A
          T_guess,                           # Rabi period
          p02_avg.min()]                     # offset

    try:
        popt, pcov = curve_fit(rabi_model, t_avg, p02_avg, p0=p0, maxfev=20000)
        A_fit, T_fit, off_fit = popt
        T_err = np.sqrt(np.diag(pcov))[1]
        fit_ok = True
        print("Fitted Rabi period  T_Rabi = %.4f ms  (+/- %.4f ms)"
              % (1000*T_fit, 1000*T_err))
        print("Effective Rabi freq Omega_eff/2pi = %.2f Hz" % (1.0/T_fit))
        print("Fitted amplitude    A = %.3f" % A_fit)
    except RuntimeError:
        fit_ok = False
        print("Rabi fit did not converge — try a longer run (more Rabi periods).")

    # -----------------------------
    # Plot
    # -----------------------------
    plt.figure(figsize=(9, 5))
    plt.plot(1000*times, p02, color="C2", alpha=0.3, lw=0.8, label="raw")
    plt.plot(1000*times[strobe], p02[strobe], "o", color="C2", ms=3,
             label="stroboscopic")
    plt.plot(1000*t_avg, p02_avg, "-", color="C3", lw=2,
             label="Floquet-averaged")
    if fit_ok:
        plt.plot(1000*t_avg, rabi_model(t_avg, *popt), "k--", lw=1.5,
                 label=r"fit: $T_\mathrm{Rabi}=%.3f$ ms" % (1000*T_fit))
    plt.xlabel("time (ms)")
    plt.ylabel(r"population in $|0,2\rangle$")
    plt.title(r"Rabi flop $|1,1\rangle \to |0,2\rangle$ at $f = %.0f$ Hz" % f_drive_hz)
    plt.ylim(0, 1.05)
    plt.legend()
    plt.tight_layout()
    plt.show()

# =============================================================
# MODE 2: RESONANCE SCAN — peak doublon transfer vs frequency
# =============================================================
elif MODE == "scan":
    span_hz  = 120.0
    n_points = 21
    # run a bit longer than one Rabi period so on-resonance .max() catches the crest
    n_periods = 2 * N_PERIODS_PER_RABI
    samples_per_period = 12

    freqs = np.linspace(f_res_hz - span_hz, f_res_hz + span_hz, n_points)

    peak_transfer = np.empty(n_points)
    for k, f in enumerate(freqs):
        _, psis = evolve(f, n_periods, samples_per_period)
        doub = np.array([np.vdot(p, D_op.dot(p)).real for p in psis])
        peak_transfer[k] = doub.max()

    plt.figure(figsize=(8, 5))
    plt.plot(freqs, peak_transfer, "o-", ms=4)
    plt.axvline(f_res_hz, color="k", ls="--", lw=1,
                label=r"$\Delta + U = %.0f$ Hz" % f_res_hz)
    plt.xlabel("drive frequency (Hz)")
    plt.ylabel(r"peak transfer  max$\,\langle n_\mathrm{d}\rangle$")
    plt.title(r"Resonance scan: $|1,1\rangle \to |0,2\rangle$")
    plt.ylim(0, 1.05)
    plt.legend()
    plt.tight_layout()
    plt.show()

    f_peak = freqs[np.argmax(peak_transfer)]
    print("expected resonance: %.1f Hz" % f_res_hz)
    print("observed peak near: %.1f Hz" % f_peak)
    print("peak transfer:      %.3f"   % peak_transfer.max())

else:
    raise ValueError("MODE must be 'time' or 'scan'")