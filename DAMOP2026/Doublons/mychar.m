function strOut = mychar(strIn)

switch strIn
    case 'pi'
        n=960;
    case 'times'
        n=215;
    case 'cdot'
        n=8901;
    case 'phi'
        n=981;
    case 'Phi'
        n=934;
    case 'theta'
        n=952;
    case 'langle'
        n=9001;
    case 'rangle'
        n=9002;
    case 'alpha'
        n=945;
    case 'beta'
        n=946;
    case 'mu'
        n=956;
    case 'hbar'
        n=8463;
    case 'psi'
        n=936;
    case 'omega'
        n=969;
    case 'Omega'
        n=937;
    case 'Gamma'
        n=915;
    case 'gamma'
        n=947;
    case 'delta'
        n=7839; %948;
    case 'Delta'
        n=916;
end

strOut = char(n);

end