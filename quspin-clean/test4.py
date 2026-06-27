import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp
from scipy.optimize import curve_fit

# =============================================================
# INITIAL AND TARGET FOCK STATES
# =============================================================
# Occupation lists. L (number of sites) and N (particle number) are inferred
# from these. Both must have the same length and the same total occupation.
state_init   = [1, 0]      # e.g. [1,1] for two atoms on separate sites
state_target = [0, 1]      # e.g. [0,2] for a doublon on site 1
# Examples:
#   single-particle hop:  state_init = [1,0,0],  state_target = [0,1,0]
#   doublon formation:    state_init = [1,1],    state_target = [0,2]

state_init   = list(state_init)
state_target = list(state_target)

if len(state_init) != len(state_target):
    raise ValueError("state_init and state_target must have the same length (L): "
                     "got %d and %d" % (len(state_init), len(state_target)))

L = len(state_init)
N = sum(state_init)

if sum(state_target) != N:
    raise ValueError("state_init and state_target must have the same particle "
                     "number N. init has N=%d, target has N=%d. The projection "
                     "would be identically zero." % (N, sum(state_target)))

# -----------------------------
# Physical parameters
# -----------------------------
J_hz     = 20.0
U_hz     = 280.0
Delta_hz = 825.0

# --- MULTI-TONE DRIVE ---------------------------------------------------------
# alpha and f_drive_list are equal-length lists, one entry per drive tone.
# Hopping is modulated as  J_t = J * (1 + sum_k alpha[k] * cos(omega[k] * t)).
alpha        = [0.0, 0.6, 0.0]
f_drive_list = [Delta_hz - U_hz, Delta_hz, Delta_hz + U_hz]
# -----------------------------------------------------------------------------

alpha        = np.atleast_1d(np.asarray(alpha, dtype=float))
f_drive_list = np.atleast_1d(np.asarray(f_drive_list, dtype=float))
if alpha.shape != f_drive_list.shape:
    raise ValueError("alpha and f_drive_list must have the same length: got %d and %d"
                     % (alpha.size, f_drive_list.size))

twopi = 2.0 * np.pi
J     = twopi * J_hz
U     = twopi * U_hz
Delta = twopi * Delta_hz
omega = twopi * f_drive_list

N_PERIODS_PER_RABI = 50
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

# sanity: the requested states must exist in this basis (they will, given the
# checks above, but guard against typos that change N silently)
if tuple(state_init) not in index:
    raise ValueError("state_init %s not found in the N=%d basis." % (state_init, N))
if tuple(state_target) not in index:
    raise ValueError("state_target %s not found in the N=%d basis." % (state_target, N))

print("L = %d, N = %d, Hilbert-space dim = %d" % (L, N, dim))
print("init   =", tuple(state_init))
print("target =", tuple(state_target))

# -----------------------------
# Hamiltonian: H(t) = H_diag + J_t * H_hop  (open chain, nearest-neighbour hop)
# -----------------------------
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

# -----------------------------
# Initial state and target projector
# -----------------------------
psi0 = np.zeros(dim, dtype=complex)
psi0[index[tuple(state_init)]] = 1.0

P_target = np.zeros((dim, dim), dtype=complex)
P_target[index[tuple(state_target)], index[tuple(state_target)]] = 1.0

# -----------------------------
# Core evolution: multi-tone drive
# -----------------------------
def evolve(n_periods, samples_per_period):
    T_ref = 1.0 / f_ref_hz

    def rhs(t, psi):
        J_t = J * (1.0 + np.sum(alpha * np.cos(omega * t)))
        return -1j * (H_diag + J_t * H_hop).dot(psi)

    t_end = n_periods * T_ref
    times = np.linspace(0.0, t_end, n_periods * samples_per_period + 1)
    sol = solve_ivp(rhs, (0.0, t_end), psi0, t_eval=times,
                    rtol=1e-9, atol=1e-9, method="RK45")
    return times, sol.y.T

# =============================================================
# TIME TRACE — projection onto target state vs time
# =============================================================
n_periods  = 3 * N_PERIODS_PER_RABI
samples_per_period = 20

times, psis = evolve(n_periods, samples_per_period)
print("final norm =", np.linalg.norm(psis[-1]))

ptar = np.array([np.vdot(p, P_target.dot(p)).real for p in psis])

# stroboscopic + floquet-averaged (over the reference-tone period)
w = samples_per_period
strobe = slice(0, len(times), w)
kernel = np.ones(w) / w
ptar_avg = np.convolve(ptar, kernel, mode="valid")
t_avg    = times[(w-1)//2 : (w-1)//2 + len(ptar_avg)]

# -----------------------------
# Fit Rabi period to the Floquet-averaged envelope
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
    print("Fitted Rabi period  T_Rabi = %.4f ms  (+/- %.4f ms)"
          % (1000*T_fit, 1000*T_err))
    print("Effective Rabi freq Omega_eff/2pi = %.2f Hz" % (1.0/T_fit))
    print("Fitted amplitude    A = %.3f" % A_fit)
except RuntimeError:
    fit_ok = False
    print("Rabi fit did not converge - try a longer run (more Rabi periods).")

# -----------------------------
# Plot
# -----------------------------
def ket(s):
    return r"|%s\rangle" % ",".join(str(x) for x in s)

tones_str = ", ".join("%.0f" % f for f in f_drive_list)
amp_str   = ", ".join("%.2f" % a for a in alpha)

plt.figure(figsize=(9, 5))
plt.plot(1000*times, ptar, color="C2", alpha=0.3, lw=0.8, label="raw")
plt.plot(1000*times[strobe], ptar[strobe], "o", color="C2", ms=3, label="stroboscopic")
plt.plot(1000*t_avg, ptar_avg, "-", color="C3", lw=2, label="Floquet-averaged")
if fit_ok:
    plt.plot(1000*t_avg, rabi_model(t_avg, *popt), "k--", lw=1.5,
             label=r"fit: $T_\mathrm{Rabi}=%.3f$ ms" % (1000*T_fit))
plt.xlabel("time (ms)")
plt.ylabel(r"projection onto $%s$" % ket(state_target))
plt.title(r"$%s \to %s$;  tones=[%s] Hz @ [%s]"
          % (ket(state_init), ket(state_target), tones_str, amp_str))
plt.ylim(0, 1.05)
plt.legend()
plt.tight_layout()
plt.savefig("test4_output.png", dpi=120)
plt.show()