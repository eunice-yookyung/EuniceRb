import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp

# =============================================================
# 1D tunneling experiment: TWO particles on L=2, U = 0,
# with a TRUNCATED basis: keep only {(1,1), (0,2)}.
#   - (2,0) is dropped, assumed pushed far off-resonant by some mechanism.
#   - NO Floquet drive, NO tilt.
# =============================================================

# -----------------------------
# System
# -----------------------------
L = 2
N = 2

J_hz = 10.0
U_hz = 0.0
TIME_EVOLVE = 0.1

PLOT_SITES = [0, 1]

twopi = 2.0 * np.pi
J = twopi * J_hz
U = twopi * U_hz

# -----------------------------
# TRUNCATED Fock basis: hand-specified, not auto-generated.
# Keep only these states; (2,0) is intentionally excluded.
# -----------------------------
basis = [(1, 1), (0, 2)]

# sanity: every kept state must have the right L and N
for s in basis:
    if len(s) != L:
        raise ValueError("state %s does not have length L=%d" % (s, L))
    if sum(s) != N:
        raise ValueError("state %s does not have N=%d particles" % (s, N))

index = {s: i for i, s in enumerate(basis)}
dim   = len(basis)

print("L = %d, N = %d, truncated dim = %d" % (L, N, dim))
print("kept basis =", basis, " (|2,0> excluded)")

# -----------------------------
# Hamiltonian in the truncated basis.
# Hopping that would leave the kept manifold (i.e. land on an excluded state
# like (2,0)) is simply dropped -- that is the projection onto the kept states.
# -----------------------------
def build_H():
    H = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        n = np.array(state)
        H[a, a] += 0.5 * U * np.sum(n * (n - 1))          # interaction (U=0 here)
        for j in range(L-1):
            if n[j] > 0:
                new = n.copy(); amp = np.sqrt(n[j])*np.sqrt(n[j+1]+1)
                new[j]-=1; new[j+1]+=1
                tgt = tuple(new)
                if tgt in index:                          # keep only in-manifold hops
                    H[index[tgt], a] += -J*amp
            if n[j+1] > 0:
                new = n.copy(); amp = np.sqrt(n[j+1])*np.sqrt(n[j]+1)
                new[j+1]-=1; new[j]+=1
                tgt = tuple(new)
                if tgt in index:
                    H[index[tgt], a] += -J*amp
    return H

H = build_H()
print("H / 2pi (Hz):\n", np.real(H)/twopi)
if not np.allclose(H, H.conj().T):
    print("WARNING: H is not Hermitian -- truncation broke a hop pair.")

# -----------------------------
# Site-density operators (within the truncated basis)
# -----------------------------
n_ops = []
for site in range(L):
    op = np.zeros((dim, dim), dtype=complex)
    for a, state in enumerate(basis):
        op[a, a] = state[site]
    n_ops.append(op)

# -----------------------------
# Initial state: |1,1> (one particle per site)
# -----------------------------
init_state = (1, 1)
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

density = np.zeros((len(times), L))
for ti, p in enumerate(psis):
    for site in range(L):
        density[ti, site] = np.vdot(p, n_ops[site].dot(p)).real

times_ms = 1000.0 * times

# -----------------------------
# Heatmap
# -----------------------------
plt.figure(figsize=(6, 6))
im = plt.imshow(density, aspect="auto", origin="lower",
                extent=[-0.5, L-0.5, times_ms[0], times_ms[-1]],
                cmap="viridis", vmin=0.0, vmax=float(N))
plt.colorbar(im, label=r"site density $\langle n_j \rangle$")
plt.xlabel("site index $j$")
plt.ylabel("time (ms)")
plt.xticks(range(L))
plt.title(r"Truncated $\{|1,1\rangle,|0,2\rangle\}$, $J=%.0f$ Hz" % J_hz)
plt.tight_layout()
plt.savefig("testJ2_heatmap.png", dpi=120)
plt.show()

# -----------------------------
# Line cuts
# -----------------------------
if PLOT_SITES:
    plt.figure(figsize=(8, 5))
    for site in PLOT_SITES:
        if site < 0 or site >= L:
            print("warning: site %d out of range, skipping" % site)
            continue
        plt.plot(times_ms, density[:, site], lw=1.8, label=r"site $j=%d$" % site)
    plt.xlabel("time (ms)")
    plt.ylabel(r"site density $\langle n_j \rangle$")
    plt.ylim(0, N + 0.05)
    plt.title(r"Truncated $\{|1,1\rangle,|0,2\rangle\}$, $J=%.0f$ Hz" % J_hz)
    plt.legend()
    plt.tight_layout()
    plt.savefig("testJ2_sites.png", dpi=120)
    plt.show()