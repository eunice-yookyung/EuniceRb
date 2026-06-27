import itertools
import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import solve_ivp


# -----------------------------
# Parameters
# -----------------------------

L = 3          # number of lattice sites
N = 2          # number of bosons (conserved — you work in fixed-N sector)

J_hz = 30.0        # bare hopping amplitude (Hz)
U_hz = 280.0       # on-site interaction (Hz) — energy cost of two bosons sharing a site
Delta_hz = 825.0   # linear tilt per site (Hz) — a "gravitational" potential gradient
f_drive_hz = 825.0 + U_hz # frequency at which you shake the hopping
alpha = 0.20       # fractional modulation depth of the drive (20%)

twopi = 2.0 * np.pi

# Convert ordinary frequencies (Hz) to angular frequencies (rad/s).
# This matters because the Schrödinger equation uses H with hbar=1,
# so every energy must be an angular frequency. Mixing the two up by 2pi
# is the single most common bug in this kind of simulation.
J = twopi * J_hz
U = twopi * U_hz
Delta = twopi * Delta_hz
omega = twopi * f_drive_hz


# -----------------------------
# Build fixed-N bosonic basis
# -----------------------------

def generate_basis(L, N):
    basis = []
    # itertools.product enumerates every possible occupation pattern,
    # allowing 0..N bosons on each of the L sites.
    for occ in itertools.product(range(N + 1), repeat=L):
        if sum(occ) == N:          # keep only states with exactly N bosons total
            basis.append(tuple(occ))
    return basis


basis = generate_basis(L, N)
# index maps an occupation tuple -> its row/column number in the matrix.
# This is the lookup you use to find where a hopped state lands.
index = {state: i for i, state in enumerate(basis)}
dim = len(basis)

print("Basis states:")
for i, state in enumerate(basis):
    print(i, state)

print("Hilbert-space dimension =", dim)


# -----------------------------
# Operators
# -----------------------------

def build_hamiltonian(J_value):
    H = np.zeros((dim, dim), dtype=complex)

    for a, state in enumerate(basis):   # loop over every basis state (column a)
        n = np.array(state)

        # --- DIAGONAL TERMS (energy of the state itself) ---

        # Interaction: U/2 * sum n_i(n_i - 1). The n(n-1) counts pairs of
        # bosons on a site; a site with 2 bosons costs U, with 3 costs 3U, etc.
        E_int = 0.5 * U * np.sum(n * (n - 1))

        # Tilt: each site i has potential energy Delta*i, so a boson on site 2
        # costs 2*Delta. np.arange(L) = [0,1,2] are the site positions.
        E_tilt = Delta * np.sum(np.arange(L) * n)

        H[a, a] += E_int + E_tilt

        # --- OFF-DIAGONAL TERMS (hopping moves bosons between sites) ---
        for j in range(L - 1):         # bonds connect site j and j+1 (open chain)

            # Term b_{j+1}^dagger b_j : remove a boson from site j, add to j+1
            if n[j] > 0:               # can only remove if site j is occupied
                new = n.copy()
                # bosonic matrix element: sqrt(n_j) from annihilation,
                # sqrt(n_{j+1}+1) from creation. This is what makes bosons
                # differ from fermions/spins.
                amp = np.sqrt(n[j]) * np.sqrt(n[j + 1] + 1)
                new[j] -= 1
                new[j + 1] += 1
                b = index[tuple(new)]  # which state we landed in
                H[b, a] += -J_value * amp   # -J is the sign convention for hopping

            # Term b_j^dagger b_{j+1} : the reverse hop (j+1 -> j)
            if n[j + 1] > 0:
                new = n.copy()
                amp = np.sqrt(n[j + 1]) * np.sqrt(n[j] + 1)
                new[j + 1] -= 1
                new[j] += 1
                b = index[tuple(new)]
                H[b, a] += -J_value * amp

    return H


H0 = build_hamiltonian(J)   # the static (undriven) Hamiltonian

# D_op counts doublons: total number of "extra" bosons piled on sites.
# 0.5*sum n(n-1) = number of doubly-occupied configurations.
# This is your observable for "have pairs formed?"
D_op = np.zeros((dim, dim), dtype=complex)
for a, state in enumerate(basis):
    n = np.array(state)
    D_op[a, a] = 0.5 * np.sum(n * (n - 1))

# -----------------------------
# Initial state: ground state of H0
# -----------------------------

E, V = np.linalg.eigh(H0)   # eigh = Hermitian eigensolver; E sorted ascending
psi0 = V[:, 0]              # column 0 = lowest-energy eigenvector = ground state

print("Ground-state energy / 2pi = %.3f Hz" % (E[0] / twopi))


# -----------------------------
# Time-dependent evolution
# -----------------------------

def H_of_t(t):
    # The drive modulates the hopping sinusoidally: J -> J(1 + alpha cos(omega t)).
    # This is "lattice shaking" / Floquet driving — periodically kicking the system.
    J_t = J * (1.0 + alpha * np.cos(omega * t))
    return build_hamiltonian(J_t)


def rhs(t, psi):
    # The time-dependent Schrödinger equation, hbar=1:
    #   d|psi>/dt = -i H(t) |psi>
    # solve_ivp integrates this forward in time.
    return -1j * H_of_t(t).dot(psi)


T = 1.0 / f_drive_hz       # one drive period (in real seconds)
n_periods = 500            # evolve for 200 drive cycles
times = np.linspace(0.0, n_periods * T, 1001)

sol = solve_ivp(
    rhs,
    (times[0], times[-1]),   # integration interval
    psi0,                    # initial condition
    t_eval=times,            # times at which to record psi
    rtol=1e-9,               # tight tolerances — important for unitary dynamics
    atol=1e-9,               # so the norm of psi doesn't drift
)

psis = sol.y.T   # sol.y is (dim x n_times); transpose -> one row per time point

"""
A note worth flagging here: 
solve_ivp with the default RK45 method does not preserve the norm of psi exactly — 
it's a general-purpose integrator, not a symplectic/unitary one. 
The tight rtol/atol = 1e-9 keep the drift small enough to ignore 
over 200 periods, which is why those tolerances matter. 
For longer runs or larger systems you'd want a proper unitary propagator 
(matrix exponential per step, or a Krylov method), but for this diagnostic it's fine. 
You could add np.linalg.norm(psis[-1]) as a sanity check — it should be very close to 1. 
"""

# Expectation values: <psi|O|psi> for each recorded time.

energies = np.array([
    np.vdot(psi, H0.dot(psi)).real    # energy measured w.r.t. the STATIC H0,
    for psi in psis                    # not H(t) — this is deliberate (see below)
])

doublons = np.array([
    np.vdot(psi, D_op.dot(psi)).real
    for psi in psis
])


# -----------------------------
# Plots
# -----------------------------

plt.figure()
# energies converted back to Hz (divide by 2pi) and referenced to t=0.
plt.plot(1000 * times, (energies - energies[0]) / twopi)   # 1000* -> milliseconds
plt.xlabel("time (ms)")
plt.ylabel("absorbed energy relative to t=0 (Hz)")
plt.title("Heating diagnostic")
plt.tight_layout()
plt.show()

plt.figure()
plt.plot(1000 * times, doublons)
plt.xlabel("time (ms)")
plt.ylabel("doublon number")
plt.title("Doublon production")
plt.tight_layout()
plt.show()