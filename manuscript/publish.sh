#!/bin/bash
asciidoctor AspNetCoreHun.adoc
asciidoctor-pdf AspNetCoreHun.adoc
asciidoctor-epub3 -a ebook-format=kf8 AspNetCoreHun.adoc