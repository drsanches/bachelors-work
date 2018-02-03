import numpy
from PIL import Image
from PIL import ImageDraw


# TODO: rewrite it
def read_array_from_file(array_path):
    array = {}
    width = 0
    height = 0
    k = 0
    t = True
    for line in open(array_path, "r").read().split("\n"):
        height = height + 1
        for unit in line.split(" "):
            if unit != "":
                array[k] = float(unit)
                k = k + 1
                if t:
                    width = width + 1
            else:
                t = False
    height = height - 1
    symbol = numpy.zeros(shape=(width, height), dtype=numpy.float)
    k = 0
    for i in range(0, height):
        for j in range(0, width):
            symbol[j, i] = array[k]
            k = k + 1
    return symbol


def write_array_in_file(array, filepath):
    file = open(filepath, 'w')
    width = array.shape[0]
    height = array.shape[1]
    for i in range(0, height):
        for j in range(0, width):
            file.write(str(array[j, i]) + " ")
        file.write("\n")
    file.close()


def array_to_image(array):
    width = array.shape[0]
    height = array.shape[1]
    image = Image.new("L", (width, height))
    draw = ImageDraw.Draw(image)
    for i in range(0, width):
        for j in range(0, height):
            draw.point((i, j), int(array[i, j] * 255))
    del draw
    return image


def image_to_array(image):
    width = image.width
    height = image.height
    array = numpy.zeros(shape=(width, height), dtype=numpy.float)
    for i in range(0, width):
        for j in range(0, height):
            array[i, j] = image.getpixel((i, j)) / 255
    return array


def resize_array(array, max_width, max_height):
    image = array_to_image(array)
    k_width = max_width / image.width
    k_height = max_height / image.height
    k = min(k_width, k_height)
    new_width = int(image.width * k)
    new_height = int(image.height * k)
    resized_image = image.resize((new_width, new_height), Image.ANTIALIAS)
    final_image = Image.new('L', (max_width, max_height), "Black")
    x1 = int(max_width / 2 - new_width / 2)
    y1 = int(max_height / 2 - new_height / 2)
    x2 = int(max_width / 2 + new_width / 2)
    y2 = int(max_height / 2 + new_height / 2)
    final_image.paste(resized_image, (x1, y1, x2, y2))
    final_array = image_to_array(final_image)
    return final_array


def prepare_for_cnn(array):
    w = array.shape[0]
    h = array.shape[1]
    X = numpy.zeros((h, w))
    for i in range(0, w):
        for j in range(0, h):
            X[j, i] = array[i, j]
    X = X.reshape(1, X.shape[0], X.shape[1], 1)
    return X
