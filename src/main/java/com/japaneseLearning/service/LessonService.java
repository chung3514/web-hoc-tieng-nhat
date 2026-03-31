package com.japaneseLearning.service;

import com.japaneseLearning.dto.LessonDTO;
import com.japaneseLearning.entity.Lesson;
import com.japaneseLearning.repository.LessonRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import java.util.List;
import java.util.stream.Collectors;

@Service
@Transactional
public class LessonService {
    
    private final LessonRepository lessonRepository;

    public LessonService(LessonRepository lessonRepository) {
        this.lessonRepository = lessonRepository;
    }

    public LessonDTO getLessonById(Long lessonId) {
        return lessonRepository.findById(lessonId)
                .map(this::convertToDTO)
                .orElseThrow(() -> new RuntimeException("Lesson not found with id: " + lessonId));
    }

    public Lesson getLessonEntityById(Long lessonId) {
        return lessonRepository.findById(lessonId)
                .orElseThrow(() -> new RuntimeException("Lesson not found with id: " + lessonId));
    }

    public List<Lesson> getAllLessons() {
        return lessonRepository.findAll();
    }

    public List<LessonDTO> getLessonsByCourseId(Long courseId) {
        return lessonRepository.findByCourse_CourseIdOrderByOrderInCourseAsc(courseId)
                .stream()
                .map(this::convertToDTO)
                .collect(Collectors.toList());
    }

    public LessonDTO createLesson(LessonDTO lessonDTO) {
        Lesson lesson = new Lesson();
        lesson.setTitle(lessonDTO.title());
        lesson.setContent(lessonDTO.content());
        lesson.setOrderInCourse(lessonDTO.orderInCourse());
        lesson.setVocabYouTubeLink(lessonDTO.vocabYouTubeLink());
        lesson.setGrammarYouTubeLink(lessonDTO.grammarYouTubeLink());
        Lesson savedLesson = lessonRepository.save(lesson);
        return convertToDTO(savedLesson);
    }

    public LessonDTO updateLesson(Long lessonId, LessonDTO lessonDTO) {
        Lesson lesson = lessonRepository.findById(lessonId)
                .orElseThrow(() -> new RuntimeException("Lesson not found with id: " + lessonId));
        
        lesson.setTitle(lessonDTO.title());
        lesson.setContent(lessonDTO.content());
        lesson.setOrderInCourse(lessonDTO.orderInCourse());
        lesson.setVocabYouTubeLink(lessonDTO.vocabYouTubeLink());
        lesson.setGrammarYouTubeLink(lessonDTO.grammarYouTubeLink());
        
        Lesson updatedLesson = lessonRepository.save(lesson);
        return convertToDTO(updatedLesson);
    }

    public void deleteLesson(Long lessonId) {
        lessonRepository.deleteById(lessonId);
    }

    private LessonDTO convertToDTO(Lesson lesson) {
        return new LessonDTO(
                lesson.getLessonId(),
                lesson.getCourse().getCourseId(),
                lesson.getTitle(),
                lesson.getContent(),
                lesson.getOrderInCourse(),
                lesson.getVocabYouTubeLink(),
                lesson.getGrammarYouTubeLink()
        );
    }
}
