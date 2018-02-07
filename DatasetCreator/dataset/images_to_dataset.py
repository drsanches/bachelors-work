import sys
import numpy
import random
from PIL import Image
from PIL import ImageDraw
from keras.utils import np_utils


def get_array(image):
    array = numpy.array(image.getdata())
    array = array.reshape(input_width, input_height, 1)
    array = 1 - array / 255
    return array

# TODO: Fix bug (works only if image.size mod 2 == 0)
def rotate(image, angle):
    frame = (int(image.width * 0.5), int(image.height * 0.5), int(image.width * 1.5), int(image.height * 1.5))
    new_image = Image.new('L', (image.width * 2, image.height * 2), "White")
    new_image.paste(image, frame)
    new_image = new_image.rotate(angle)
    new_image = new_image.transform(image.size, Image.EXTENT, frame)
    new_array = get_array(new_image)
    return new_array

def random_dots(image, count):
    new_image = image.copy()
    draw = ImageDraw.Draw(new_image)
    for i in range(0, count):
        x = random.randint(0, new_image.width - 1)
        y = random.randint(0, new_image.height - 1)
        color = random.randint(0, 255)
        draw.point((x, y), fill=color)
    del draw
    new_array = get_array(new_image)
    return new_array

def scaling(image, size):
    scaled_image = image.copy()
    scaled_image = scaled_image.resize((int(image.width * size[0]), int(image.width * size[1])), Image.ANTIALIAS)
    new_image = Image.new('L', (image.width, image.height), "White")
    w = image.width
    h = image.height
    n_w = int(image.width * size[0])
    n_h = int(image.height * size[1])
    zero_point = (int(w / 2 - n_w / 2), int(h / 2 - n_h / 2))
    new_image.paste(scaled_image, zero_point)
    new_array = get_array(new_image)
    return new_array




input_width = 50
input_height = 50

try:
    filename = sys.argv[1]
    directory = sys.argv[2]
    output_filename = sys.argv[3]
except:
    filename = "symbols.txt"
    directory = "images"
    output_filename = "dataset.npz"

X_train = []
Y_train = []
X_test = []
Y_test = []
f = open(filename, 'r')
count = 0
for line in f:
    symbol = line[0: len(line) - 1]
    if (symbol != ""):
        image = Image.open(directory + "\\" + str(count) + ".png")
        image = image.resize((input_width, input_height), Image.ANTIALIAS)
        image = image.convert('L')

        # Normal
        array = get_array(image)
        X_train.append(array)
        Y_train.append(count)

        # Rotated
        array = rotate(image, 5)
        X_train.append(array)
        Y_train.append(count)
        array = rotate(image, -5)
        X_train.append(array)
        Y_train.append(count)

        # Noise
        array = random_dots(image, 50)
        X_train.append(array)
        Y_train.append(count)

        # Scaling
        array = scaling(image, (1.2, 1.2))
        X_train.append(array)
        Y_train.append(count)
        array = scaling(image, (1.4, 1.4))
        X_train.append(array)
        Y_train.append(count)

        # Noise for test
        array = random_dots(image, 50)
        X_test.append(array)
        Y_test.append(count)

        print(symbol)
        count = count + 1
f.close()

X_train = numpy.array(X_train).astype('float32')
Y_train = np_utils.to_categorical(Y_train, count)
X_test = numpy.array(X_test).astype('float32')
Y_test = np_utils.to_categorical(Y_test, count)

print(X_train.shape)
print(Y_train.shape)
print(X_test.shape)
print(Y_test.shape)

numpy.savez(output_filename, X_train=X_train, Y_train=Y_train, X_test=X_test, Y_test=Y_test)
