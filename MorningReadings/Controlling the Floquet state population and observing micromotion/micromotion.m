% -----------------------------------
% basis:
% -----------------------------------
% singlet
% triplet
% D+
% D-

% -----------------------------------
% parameters
% -----------------------------------
t = 1; % tunneling
U_list = (-10:.01:10) * t;

% -----------------------------------
% start diagonalization
% -----------------------------------
E_list = zeros(4, length(U_list));
vec_list = zeros(4,4,length(U_list));
for u = 1:length(U_list)

    U = U_list(u);
    H = zeros(4);

    h_elements = [
            1,3,-2*t; ...
            3,1,-2*t; ...
            3,3,U; ...
            4,4,U
        ];

    for i = 1:height(h_elements)
        H(h_elements(i, 1), h_elements(i, 2)) = h_elements(i, 3);
    end

    [vecs, vals] = eig(H, 'vector');
    
    if U <= 0
        ordering = 1:4;
    else
        ordering = [1,3,2,4];
    end

    E_list(:, u) = vals(ordering);
    vec_list(:,:,u) = vecs(ordering, :);

end

figure
plot(U_list, E_list, 'linewidth', 2)
xlabel('U/t'), ylabel('E/t'), grid on

disp('%%%%%%%%%%%%%%%%%%%%%%%%%%%')
vec_list(:,:,1),vec_list(:,:,end)