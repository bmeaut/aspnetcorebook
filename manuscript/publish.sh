#!/bin/bash
asciidoctor Book.adoc
asciidoctor-pdf Book.adoc
asciidoctor-epub3 -a ebook-format=kf8 Book.adoc