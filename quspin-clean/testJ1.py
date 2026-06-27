import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp

# =============================================================
# 1D tunneling experiment: TWO particles on L=2, U = 0
#   - NO Floquet drive (static J)
#   - NO tilt (Delta = 0)
#   - NO on-site interaction (U = 0)
#   - both particles start on the same (center/left) site
# =============================================================

# -----------------------------
# System
# -----------------------------
L = 2          # number of sites
N = 2          # number of particles

J_hz = 10.0    # tunneling strength (Hz)
U_hz = 0.0     # on-site interaction (Hz) -- set to zero here
TIME_EVOLVE = 0.1   # total evolution time (seconds)

# Line-cut option: site density vs time for specified site(s).
# Set to a list of site indices, e.g. [0] or [0, 1]; None/[] to skip.
PLOT_SITES = [0, 1]

twopi = 2.0 * np.pi
J = twopi * J_hz    # angular frequency (rad/s)
U = twopi * U_hz

# -----------------------------
# Fixed-N bosonic basis
# -----------------------------
def generate_basis(L, N):
    return [tuple(occ) for occ in itertools.product(range(N + 1), repeat=L)
            if sum(occ) == N]
basis = generate_basis(L, N)
index = {s: i for i, s in enumerate(basis)}
dim   = len(basis)

print("L = %d, N = %d, dim = %d" % (L, N, dim))
print("basis =", basis)

# -----------------------------
# Hamiltonian: nearest-neighbour hopping + on-site interaction (U=0 here).
# Open chain. Delta = 0 (no tilt).
# -----------------------------
def build_H():
    H = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        n = np.array(state)
        # diagonal: interaction only (U=0 -> vanishes, kept for generality)
        H[a, a] += 0.5 * U * np.sum(n * (n - 1))
        # hopping
        for j in range(L-1):
            if n[j] > 0:
                new = n.copy(); amp = np.sqrt(n[j])*np.sqrt(n[j+1]+1)
                new[j]-=1; new[j+1]+=1
                H[index[tuple(new)], a] += -J*amp
            if n[j+1] > 0:
                new = n.copy(); amp = np.sqrt(n[j+1])*np.sqrt(n[j]+1)
                new[j+1]-=1; new[j]+=1
                H[index[tuple(new)], a] += -J*amp
    return H

H = build_H()
print(H)

# -----------------------------
# Site-density operators n_j (diagonal in Fock basis)
# -----------------------------
n_ops = []
for site in range(L):
    op = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        op[a, a] = state[site]
    n_ops.append(op)

# -----------------------------
# Initial state: both particles on the center site.
# For L=2, L//2 = 1; but "both on one site" is the natural start. Use site 0
# (left) so the walk spreads rightward; change `start_site` to taste.
# -----------------------------
start_site = 0
init_state = tuple(N if s == start_site else 0 for s in range(L))
psi0 = np.zeros(dim, dtype=complex)
psi0[index[init_state]] = 1.0
print("initial state =", init_state)

# -----------------------------
# Time evolution
# -----------------------------
def rhs(t, psi):
    return -1j * H.dot(psi)

n_times = 400
times = np.linspace(0.0, TIME_EVOLVE, n_times)
sol = solve_ivp(rhs, (0.0, TIME_EVOLVE), psi0, t_eval=times,
                rtol=1e-10, atol=1e-10, method="RK45")
psis = sol.y.T
print("final norm =", np.linalg.norm(psis[-1]))

# density[t, site] = <n_site>(t)   (now ranges 0..N, not 0..1)
density = np.zeros((len(times), L))
for ti, p in enumerate(psis):
    for site in range(L):
        density[ti, site] = np.vdot(p, n_ops[site].dot(p)).real

times_ms = 1000.0 * times    # seconds -> ms

# -----------------------------
# Heatmap: density vs (site, time)
# -----------------------------
plt.figure(figsize=(6, 6))
im = plt.imshow(density, aspect="auto", origin="lower",
                extent=[-0.5, L-0.5, times_ms[0], times_ms[-1]],
                cmap="viridis", vmin=0.0, vmax=float(N))
plt.colorbar(im, label=r"site density $\langle n_j \rangle$")
plt.xlabel("site index $j$")
plt.ylabel("time (ms)")
plt.xticks(range(L))
plt.title(r"Two particles, $L=%d$, $U=%.0f$, $J=%.0f$ Hz" % (L, U_hz, J_hz))
plt.tight_layout()
plt.savefig("testJ1_heatmap.png", dpi=120)
plt.show()

# -----------------------------
# Line cuts: density on specified site(s) vs time
# -----------------------------
if PLOT_SITES:
    plt.figure(figsize=(8, 5))
    for site in PLOT_SITES:
        if site < 0 or site >= L:
            print("warning: site %d out of range [0, %d], skipping" % (site, L-1))
            continue
        plt.plot(times_ms, density[:, site], lw=1.8, label=r"site $j=%d$" % site)
    plt.xlabel("time (ms)")
    plt.ylabel(r"site density $\langle n_j \rangle$")
    plt.ylim(0, N + 0.05)
    plt.title(r"Site occupation vs time, $L=%d$, $U=%.0f$, $J=%.0f$ Hz"
              % (L, U_hz, J_hz))
    plt.legend()
    plt.tight_layout()
    plt.savefig("testJ1_sites.png", dpi=120)
    plt.show()