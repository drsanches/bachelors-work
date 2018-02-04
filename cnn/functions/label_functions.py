def label_creator(cnn_out):
    cnn_out = cnn_out[0]
    result = cnn_out.argmax()
    if float(cnn_out[result]) < 0.95:
        return "Error"
    else:
        return result
