%%
figure
for lambda = [25, 100]
    k = 2:2*lambda; % Define a range of k values for the Poisson distribution
    poissonProbabilities = exp(-lambda) * (lambda.^k) ./ factorial(k); % Calculate probabilities
    % exp(-lambda) * lambda^k / factorial(k);
    plot(k / lambda, poissonProbabilities / max(poissonProbabilities(~isinf(poissonProbabilities))) , 'o-','DisplayName',num2str(lambda)), hold on
    xlabel('Number of events'), ylabel('Probability of occurrence')
    legend
    grid on
end
%% signal to noise ratio
signal = [25, 100];
noise = sqrt(signal);
signal_to_noise_ratio = signal ./ noise;
%% Array example
atoms = ones(200,200);
figure,myimshow_pad(atoms)

%% Random array
rng(42)
p_occupied = 0.5;
r = rand(size(atoms));
atoms_rand = r < p_occupied;
figure,myimshow_pad(atoms_rand)

%% Fluorescence
photons_per_atom = 25;
fluorescence = atoms_rand .* photons_per_atom; % Simulate fluorescence based on random occupancy
figure, nexttile, myimshow_pad(atoms_rand), nexttile, myimshow_pad(fluorescence);

fluorescence_poiss = poissrnd(fluorescence);
% Display the Poisson-distributed fluorescence
figure
ax1 = nexttile; myimshow_pad(atoms_rand), 
ax2 = nexttile; myimshow_pad(fluorescence_poiss);
linkaxes([ax1, ax2], 'xy')

%% camera background 
camera_background = randn(size(atoms)) * 1;
fluorescence_poiss = poissrnd(fluorescence) + camera_background;
% Display the Poisson-distributed fluorescence
figure
ax1 = nexttile; myimshow(atoms_rand), 
ax2 = nexttile; myimshow(fluorescence_poiss);
linkaxes([ax1, ax2], 'xy')
figure,histogram(fluorescence_poiss(:))

%% envelope
photons_per_atom = 25;
% envelope = ones(size(atoms)) .* photons_per_atom;
% envelope(:, 1:100) = 10; % sharp cutoff
[X,Y] = meshgrid(1:200,1:200);
envelope = X/200 * photons_per_atom;

fluorescence = atoms_rand .* envelope; % Simulate fluorescence based on random occupancy
figure, nexttile, myimshow_pad(atoms_rand), nexttile, myimshow_pad(fluorescence);

fluorescence_poiss = poissrnd(fluorescence) + camera_background;
% Display the Poisson-distributed fluorescence
figure
ax1 = nexttile; myimshow_pad(atoms_rand), 
ax2 = nexttile; myimshow_pad(fluorescence_poiss);
linkaxes([ax1, ax2], 'xy')
figure,histogram(fluorescence_poiss(:))

function myimshow_pad(image)
pad = [50, 50];
myimshow(padarray(image,pad,0,'both'))
end