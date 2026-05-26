tic
% close

% Physical constants for Rb 87
mass            = 1.443e-25;
a_lattice       = 680e-9;
k_lattice       = 2*pi/(2*a_lattice);
h               = 6.626e-34;
hbar            = h / (2*pi);
ER              = hbar^2* k_lattice^2 / (2*mass);
joules_2_Hz     = 1/h;
a0              = 5.29e-11; % Bohr radius
a_s             = 95 * a0;

% Diagonalizing periodic lattice Hamiltonian
dq              = 1/20;
qvec            = (dq/2):dq:(1-dq/2); % In units of hbar k_lat
nq              = length(qvec);
lmax            = 20; % Results in 2n+1 frequencies
lvalues         = (-lmax:lmax)';
nl              = length(lvalues);
nbands          = 3; % Get lowest bands only
Vlat            = 100;
x               = linspace(-5,5,1e3)'; % x position in units of lattice spacing
plot_band       = 1; % band to plot

if nbands < max(plot_band)
    disp('Updating n bands to be at least larger than plot bands')
    nbands = max(plot_band);
end

E               = zeros(nl,length(qvec));
C               = zeros(nq,nl,nbands); % Coefficients

disp('Diagonalizing matrices')
% figure
for iq = 1:nq
    q = qvec(iq);
    H = make_hamiltonian(lmax,q,Vlat);
    [c, e] = eig(H, 'vector');
    [e, ind] = sort(e);
    c = c(:,ind);
    for in = 1:nl
        sg = sign(c(lmax+1,in));
        c(:,in) = c(:,in) * sg;
    end
    E(:,iq) = e;
    C(iq,:,:) = c(:,1:nbands);
end

% Generate the Bloch functions
disp('Making Bloch functions')
k = pi; % since x is in units of lattice spacing, k=pi/a=pi
Phi = zeros(length(x), length(qvec), nbands);
for iq = 1:nq % index over q
    q = qvec(iq);
    for in = 1:nbands
        for il = 1:nl % sum over l frequencies
            l = lvalues(il);
            ccoeff = C(iq,il,in);
            % Phi(:,iq,in) = Phi(:,iq,in) + ccoeff * exp(1i * (pi * q * x + 2 * k * l * x));
            if mod(in,2)==1
                Phi(:,iq,in) = Phi(:,iq,in) + ccoeff * cos(pi * q * x + 2 * k * l * x);
            else
                Phi(:,iq,in) = Phi(:,iq,in) + ccoeff * sin(pi * q * x + 2 * k * l * x);
            end
        end
    end
end

% Make Wannier functions
disp('Generating Wannier functions')
phi = Phi(:,:,plot_band);

wn = sum(phi,2) / nq;

% Plotting the bands
figure
plot([flip(-qvec),qvec],[fliplr(E(1:3,:)),E(1:3,:)],'k')

figure
plot(x,real(phi(:,1)))
hold on
plot(x,real(phi(:,end)))

figure
plot(x,real(wn))

%
disp('Calculating J, U')
% Calculate U
U = (trapz(x, abs(wn).^4))^3 * 8/pi * a_s/a_lattice;
E2 = E(3,:);
E1 = E(2,:);
E0 = E(1,:);
Egap = min(E1) - max(E0);
Egap2 = min(E2) - max(E0);

% Get tunneling
J = (max(E0) - min(E0))/4;
tt = toc;

disp('    ***************')
disp(['    J = ' num2str(J * ER * joules_2_Hz) ' Hz @ ' num2str(Vlat) ' ER'])
disp(['    U = ' num2str(U * ER * joules_2_Hz) ' Hz @ ' num2str(Vlat) ' ER'])
disp(['    E_gap = ' num2str(Egap * ER * joules_2_Hz / 1000) ' kHz @ ' num2str(Vlat) ' ER'])
disp(['    E_gap2 = ' num2str(Egap2 * ER * joules_2_Hz / 1000) ' kHz @ ' num2str(Vlat) ' ER'])

disp(['    ' num2str(round(tt*100)/100) ' seconds elapsed.'])

%% Save wannier function results

wannier_results = [];
tic;

for Vlat = 1:.5:50
    E               = zeros(nl,length(qvec));
    C               = zeros(nq,nl,nbands); % Coefficients
    
    for iq = 1:nq
        q = qvec(iq);
        H = make_hamiltonian(lmax,q,Vlat);
        [c, e] = eig(H, 'vector');
        [e, ind] = sort(e);
        c = c(:,ind);
        for in = 1:nl
            sg = sign(c(lmax+1,in));
            c(:,in) = c(:,in) * sg;
        end
        E(:,iq) = e;
        C(iq,:,:) = c(:,1:nbands);
    end
    
    % Generate the Bloch functions
    k = pi; % since x is in units of lattice spacing, k=pi/a=pi
    Phi = zeros(length(x), length(qvec), nbands);
    for iq = 1:nq % index over q
        q = qvec(iq);
        for in = 1:nbands
            for il = 1:nl % sum over l frequencies
                l = lvalues(il);
                ccoeff = C(iq,il,in);
                % Phi(:,iq,in) = Phi(:,iq,in) + ccoeff * exp(1i * (pi * q * x + 2 * k * l * x));
                if mod(in,2)==1
                    Phi(:,iq,in) = Phi(:,iq,in) + reshape(ccoeff * cos(pi * q * x + 2 * k * l * x), [], 1);
                else
                    Phi(:,iq,in) = Phi(:,iq,in) + reshape(ccoeff * sin(pi * q * x + 2 * k * l * x), [], 1);
                end
            end
        end
    end
    
    phi = Phi(:,:,plot_band);
    wn = sum(phi,2)' / nq;
    x = reshape(x, 1, []);
    
    % Calculate U
    U = (trapz(x, abs(wn).^4))^3 * 8/pi * a_s/a_lattice;
    E2 = E(3,:);
    E1 = E(2,:);
    E0 = E(1,:);
    Egap = min(E1) - max(E0);
    Egap2 = min(E2) - max(E0);

    
    % Get tunneling
    J = (max(E0) - min(E0))/4;    

    disp(['V_lat = ' num2str(Vlat) ', J = ' num2str(J * ER * joules_2_Hz) ' Hz, ' ...
        'U = ' num2str(U * ER * joules_2_Hz) ' Hz, ' ...
        'gap = ' num2str(Egap * ER * joules_2_Hz/1000) ' kHz, ' ...
        'gap2 = ' num2str(Egap2 * ER * joules_2_Hz/1000) ' kHz.'])

    wannier_results = [wannier_results; table(Vlat, J, U, Egap, Egap2, x, wn)];

end
tt = toc;
disp(['    ' num2str(round(tt*100)/100) ' seconds elapsed.'])

%% Plotting the results
load('wannier_results.mat')
Vlat_ = 1:.1:25;
wannier_results = wannier_results(1:max(Vlat_),:);
J_ = interp1(wannier_results.Vlat,wannier_results.J,Vlat_,'cubic') * ER * joules_2_Hz;
U_ = interp1(wannier_results.Vlat,wannier_results.U,Vlat_,'cubic') * ER * joules_2_Hz;
Egap_ = interp1(wannier_results.Vlat,wannier_results.Egap,Vlat_) * ER * joules_2_Hz;

figure('Color',[1,1,1])%('Position',[0.1556 0.3741 0.7894 0.2618])
tiledlayout(3,1,'TileSpacing','compact','Padding','compact')
nexttile
plot(wannier_results.Vlat,wannier_results.J * ER * joules_2_Hz,'or','LineWidth',1)
hold on
plot(Vlat_, J_,'-k','LineWidth',1)
xlabel('Lattice depth (ER)')
ylabel('Bare tunneling J (Hz)')
legend('Numeric','Interpolated','FontSize',12)
my_axes

nexttile
plot(wannier_results.Vlat,wannier_results.U * ER * joules_2_Hz,'or','LineWidth',1)
hold on
plot(Vlat_, U_,'-k','LineWidth',1)
hold on
xlabel('Lattice depth (ER)')
ylabel('Bare on-site interaction U (Hz)')
my_axes

nexttile
plot(wannier_results.Vlat,wannier_results.Egap * ER * joules_2_Hz/1000,'or','LineWidth',1)
hold on
plot(Vlat_, Egap_/1000,'-k','LineWidth',1)
xlabel('Lattice depth (ER)')
ylabel('Bare gap (kHz)')
my_axes

% U1 = wannier_results.U(wannier_results.Vlat==4);
% U2 = wannier_results.U(wannier_results.Vlat==45);
% U3 = wannier_results.U(wannier_results.Vlat==9);
% U = U1^(1/3) * U2^(1/3) * U3^(1/3) * ER * joules_2_Hz


%%
function H = make_hamiltonian(lmax, q, V0)

H = zeros(2*lmax+1,2*lmax+1);
lvec = -lmax:lmax;

for i=1:length(lvec)
    l=lvec(i);
    H(i,i) = (2*l + q)^2;

    if i<2*lmax+1
        H(i,i+1) = -1/4 * V0;
        H(i+1,i) = -1/4 * V0;
    end
end

end

function my_axes(ax)

if nargin < 1
    ax = gca;
end

ax.FontSize=16;
end