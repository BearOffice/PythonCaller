from sklearn.datasets import load_digits
from sklearn.manifold import TSNE
import csconnector as csc

csc.init_environment()

digits = load_digits()
tsne = TSNE(init="pca", learning_rate="auto")
digits_tsne = tsne.fit_transform(digits.data)

csc.set_output(digits_tsne.tolist())
