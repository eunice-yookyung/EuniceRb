%% load images

addpath('/Users/yookyunglee/Documents/GitHub/RbRepository/software/atom_fitting/other_analysis_code')

fignames = dir(fullfile('guppy_images', '*.fig'));
n = length(fignames);
images = zeros(2592, 1944, n);
for i = 1:n
    fig = openfig(fignames(i).name, 'invisible');
    imgObj = findall(fig, 'Type', 'Image');
    for k = 1:numel(imgObj)
        fprintf('Image %d: CData size = [%s]\n', ...
            k, num2str(size(imgObj(k).CData)));
    end
    images(:,:,i) = imgObj(1).CData;
end

%% choose two images

im1 = images(:,:,1);
im2 = images(:,:,end);

figure
ax1 = nexttile;
myimshow(im1)
ax2 = nexttile;
myimshow(im2)
linkaxes([ax1, ax2], 'xy')

%% mask
mask = ones(size(im1));
for i = 1:2
    if i == 1
        colx = [1167, 1322];
        rowx = [419 585];
    else
        colx = [935, 1105];
        rowx = [1350, 1515];
    end
    mask(rowx(1):rowx(2), colx(1):colx(2)) = 0;
end

figure
ax1 = nexttile;
myimshow(im1.*mask)
ax2 = nexttile;
myimshow(im2.*mask)
linkaxes([ax1, ax2], 'xy')

%% ft
f1 = fftshift(fft2((im1-mean(im1(:)))));
f2 = fftshift(fft2((im2-mean(im2(:)))));
cl = [10^4,10^7];
figure
ax1 = nexttile;
myimshow(abs(f1)), set(gca,'ColorScale','log'), clim(cl)
ax2 = nexttile;
myimshow(abs(f2)), set(gca,'ColorScale','log'), clim(cl)
linkaxes([ax1, ax2], 'xy')

%% heavy low pass

fmask = ones(size(im1));
[X,Y] = meshgrid(1:size(im1,2), 1:size(im1,1));
x0 = (size(im1,2)+1)/2;
y0 = (size(im1,1)+1)/2;
fR = size(im1)*.035;
circ = (X-x0).^2/fR(2)^2+(Y-y0).^2/fR(1)^2>1;
fmask(circ) = 0;

f1 = fftshift(fft2(im1)) .* fmask;
f2 = fftshift(fft2(im2)) .* fmask;
l1 = ifft2(ifftshift(f1));
l2 = ifft2(ifftshift(f2));
l1(l1<0) = 0;
l2(l2<0) = 0;

cl = [10^4,10^7];
figure
ax1 = nexttile;
myimshow(abs(l1)), %set(gca,'ColorScale','log'), clim(cl)
ax2 = nexttile;
myimshow(abs(l2)), %set(gca,'ColorScale','log'), clim(cl)
linkaxes([ax1, ax2], 'xy')

%% subtract low pass
s1 = real(im1-l1);
s2 = real(im2-l2);
cl=[25,30];
figure
ax1 = nexttile;
myimshow(s1), %clim(cl)
ax2 = nexttile;
myimshow(s2), %clim(cl)
linkaxes([ax1, ax2], 'xy')
ax3 = nexttile;
myimshow(im1), %clim(cl)
ax4 = nexttile;
myimshow(im2), %clim(cl)
linkaxes([ax1, ax2,ax3,ax4], 'xy')

figure
nexttile,plot(sum(s1,2)),nexttile,plot(sum(im1,2))
%% look at fft
f1 = fftshift(fft2(s1));
f2 = fftshift(fft2(s2));
cl = [10^4,10^7];
figure
ax1 = nexttile;
myimshow(abs(f1)), set(gca,'ColorScale','log'), clim(cl)
ax2 = nexttile;
myimshow(abs(f2)), set(gca,'ColorScale','log'), clim(cl)
linkaxes([ax1, ax2], 'xy')

%% fourier filter
fmask = ones(size(im1));
[X,Y] = meshgrid(1:size(im1,2), 1:size(im1,1));
x0 = (size(im1,2)+1)/2;
y0 = (size(im1,1)+1)/2;
fR = size(im1)*.1;
circ = (X-x0).^2/fR(2)^2+(Y-y0).^2/fR(1)^2>1;
fmask(circ) = 0;
% fmask(round(y0-50):round(y0+50),:)=0;
% fmask(:,round(x0-30):round(x0+30))=0;

f1 = fftshift(fft2(s1)).* fmask;
f2 = fftshift(fft2(s2)).* fmask;

cl = [10^4,10^5];

figure
ax1 = nexttile;
myimshow(abs(f1)), set(gca,'ColorScale','log'), clim(cl)
ax2 = nexttile;
myimshow(abs(f2)), set(gca,'ColorScale','log'), clim(cl)
linkaxes([ax1, ax2], 'xy')

%%
f1 = fftshift(fft2(s1)).* fmask;
f2 = fftshift(fft2(s2)).* fmask;
i1 = ifft2(ifftshift(f1));
i2 = ifft2(ifftshift(f2));
i1(i1<0) = 0; i1=real(i1);
i2(i2<0) = 0; i2=real(i2);

cl = [10^4,10^7];

figure
ax1 = nexttile;
myimshow(i1), %set(gca,'ColorScale','log'), %clim(cl)
ax2 = nexttile;
myimshow(i2), %set(gca,'ColorScale','log'), %clim(cl)
linkaxes([ax1, ax2], 'xy')

% figure,plot(sum(i1,2))
%%
